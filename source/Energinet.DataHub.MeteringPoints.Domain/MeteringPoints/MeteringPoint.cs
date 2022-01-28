// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Exceptions;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Exceptions;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules.Connect;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules.Disconnect;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public class MeteringPoint : AggregateRootBase
    {
        private readonly ExchangeGridAreas? _exchangeGridAreas;
        private MasterData _masterData;
        private MeteringPointId? _parentMeteringPoint;

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind Address in main constructor
        private MeteringPoint() { }
#pragma warning restore 8618

        private MeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            MeteringPointType meteringPointType,
            GridAreaLinkId gridAreaLinkId,
            MasterData masterData)
        {
            Id = id;
            GsrnNumber = gsrnNumber;
            MeteringPointType = meteringPointType;
            GridAreaLinkId = gridAreaLinkId;
            _masterData = masterData;

            RaiseMeteringPointCreated();
        }

        private MeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            GridAreaLinkId gridAreaLinkId,
            ExchangeGridAreas exchangeGridAreas,
            MasterData masterData)
        {
            Id = id;
            GsrnNumber = gsrnNumber;
            MeteringPointType = MeteringPointType.Exchange;
            GridAreaLinkId = gridAreaLinkId;
            _exchangeGridAreas = exchangeGridAreas ?? throw new ArgumentNullException(nameof(exchangeGridAreas));
            _masterData = masterData;

            RaiseMeteringPointCreated();
        }

        public MeteringPointId Id { get; }

        public GsrnNumber GsrnNumber { get; }

        public MeteringPointType MeteringPointType { get; }

        public MasterData MasterData => _masterData;

        internal GridAreaLinkId GridAreaLinkId { get; }

        internal ConnectionState ConnectionState { get; set; } = ConnectionState.New();

        internal EnergySupplierDetails? EnergySupplierDetails { get; private set; }

        public static BusinessRulesValidationResult CanCreate(MeteringPointType type, MasterData masterData, MasterDataValidator validator)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            return validator.CheckRulesFor(type, masterData);
        }

        public static MeteringPoint CreateExchange(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            GridAreaLinkId gridAreaLinkId,
            ExchangeGridAreas exchangeGridAreas,
            MasterData masterData)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (gsrnNumber == null) throw new ArgumentNullException(nameof(gsrnNumber));
            if (gridAreaLinkId == null) throw new ArgumentNullException(nameof(gridAreaLinkId));
            if (exchangeGridAreas == null) throw new ArgumentNullException(nameof(exchangeGridAreas));
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));

            return new MeteringPoint(id, gsrnNumber, gridAreaLinkId, exchangeGridAreas, masterData);
        }

        public static MeteringPoint Create(MeteringPointId id, GsrnNumber gsrnNumber, MeteringPointType meteringPointType, GridAreaLinkId gridAreaLinkId, MasterData masterData)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (gsrnNumber == null) throw new ArgumentNullException(nameof(gsrnNumber));
            if (meteringPointType is null) throw new ArgumentNullException(nameof(meteringPointType));
            if (gridAreaLinkId == null) throw new ArgumentNullException(nameof(gridAreaLinkId));
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));
            return new MeteringPoint(id, gsrnNumber, meteringPointType, gridAreaLinkId, masterData);
        }

        public void CloseDown()
        {
            ConnectionState = ConnectionState.ClosedDown();
        }

        public void SetEnergySupplierDetails(EnergySupplierDetails energySupplierDetails)
        {
            if (energySupplierDetails == null) throw new ArgumentNullException(nameof(energySupplierDetails));
            if (EnergySupplierDetails?.StartOfSupply != null) return;

            if (MeteringPointType.IsAccountingPoint == false)
            {
                throw new CannotAssignEnergySupplierException();
            }

            EnergySupplierDetails = energySupplierDetails;
            AddDomainEvent(new EnergySupplierDetailsChanged(Id.Value, EnergySupplierDetails.StartOfSupply));
        }

        public virtual BusinessRulesValidationResult ConnectAcceptable(ConnectionDetails connectionDetails)
        {
            var rules = new Collection<IBusinessRule>
            {
                new MeterMustBePhysicalRule(MeteringPointType, _masterData.MeteringConfiguration),
                new MustBeCoupledToParentRule(MeteringPointType, _parentMeteringPoint),
                new MeteringPointMustHavePhysicalStateNewRule(GsrnNumber, MeteringPointType, ConnectionState.PhysicalState),
                new MustHaveEnergySupplierRule(this, connectionDetails),
            };

            return new BusinessRulesValidationResult(rules);
        }

        public virtual void Connect(ConnectionDetails connectionDetails)
        {
            if (connectionDetails == null) throw new ArgumentNullException(nameof(connectionDetails));
            if (!ConnectAcceptable(connectionDetails).Success)
            {
                throw MeteringPointConnectException.Create(Id, GsrnNumber);
            }

            ConnectionState = ConnectionState.Connected(connectionDetails.EffectiveDate);
            AddDomainEvent(new MeteringPointConnected(Id.Value, GsrnNumber.Value, connectionDetails.EffectiveDate));
        }

        public BusinessRulesValidationResult DisconnectAcceptable(ConnectionDetails connectionDetails)
        {
            var rules = new Collection<IBusinessRule>
            {
                new PhysicalStateMustBeConnectedRule(ConnectionState),
                new MustHaveEnergySupplierRule(this, connectionDetails),
            };
            return new BusinessRulesValidationResult(rules);
        }

        public void Disconnect(ConnectionDetails connectionDetails)
        {
            if (connectionDetails == null) throw new ArgumentNullException(nameof(connectionDetails));
            if (!DisconnectAcceptable(connectionDetails).Success)
            {
                throw MeteringPointDisconnectException.Create(Id, GsrnNumber);
            }

            ConnectionState = ConnectionState.Disconnected(connectionDetails.EffectiveDate);
            AddDomainEvent(new MeteringPointDisconnected(Id.Value, GsrnNumber.Value, connectionDetails.EffectiveDate));
        }

        public BusinessRulesValidationResult CanUpdateMasterData(MasterData updatedMasterData, MasterDataValidator validator)
        {
            if (updatedMasterData == null) throw new ArgumentNullException(nameof(updatedMasterData));
            if (validator == null) throw new ArgumentNullException(nameof(validator));

            var errors = new List<ValidationError>();
            if (IsClosedDown())
            {
                errors.Add(new ClosedDownMeteringPointCannotBeChangedError());
            }

            errors.AddRange(validator.CheckRulesFor(this, updatedMasterData).Errors);

            return new BusinessRulesValidationResult(errors);
        }

        public void UpdateMasterData(MasterData updatedMasterData, MasterDataValidator validator)
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            var validationResult = CanUpdateMasterData(updatedMasterData, validator);
            if (validationResult.Success == false)
            {
                throw new MasterDataChangeException(validationResult.Errors);
            }

            _masterData = updatedMasterData ?? throw new ArgumentNullException(nameof(updatedMasterData));
            RaiseMasterDataWasUpdatedEvent();
        }

        internal void SetParent(MeteringPointId? parentId)
        {
            if (parentId is not null)
            {
                _parentMeteringPoint = parentId;
                AddDomainEvent(new CoupledToParent(Id.Value, parentId.Value));
            }
        }

        internal void RemoveParent()
        {
            if (_parentMeteringPoint is null) return;
            AddDomainEvent(new DecoupledFromParent(Id.Value, _parentMeteringPoint.Value));
            _parentMeteringPoint = null;
        }

        private bool IsClosedDown()
        {
            return ConnectionState.PhysicalState == PhysicalState.ClosedDown;
        }

        private void RaiseMeteringPointCreated()
        {
            var @event = new Events.MeteringPointCreated(
                MeteringPointType.Name,
                Id.Value,
                GsrnNumber.Value,
                GridAreaLinkId.Value,
                _masterData.MeteringConfiguration.Method.Name,
                _masterData.ProductType.Name,
                _masterData.ReadingOccurrence.Name,
                _masterData.UnitType?.Name!,
                _masterData.SettlementMethod?.Name!,
                _masterData.NetSettlementGroup?.Name!,
                _masterData.Address.City,
                _masterData.Address.Floor,
                _masterData.Address.Room,
                _masterData.Address.BuildingNumber,
                _masterData.Address.CountryCode?.Name,
                _masterData.Address.MunicipalityCode,
                _masterData.Address.PostCode,
                _masterData.Address.StreetCode,
                _masterData.Address.StreetName,
                _masterData.Address.CitySubDivision,
                _masterData.Address.IsActual.GetValueOrDefault(),
                _masterData.Address.GeoInfoReference,
                _masterData.PowerPlantGsrnNumber?.Value,
                _masterData.Address.LocationDescription,
                _masterData.MeteringConfiguration.Meter?.Value,
                _masterData.PowerLimit?.Ampere,
                _masterData.PowerLimit?.Kwh,
                _masterData.EffectiveDate.DateInUtc,
                _masterData.DisconnectionType?.Name!,
                _masterData.ConnectionType?.Name,
                _masterData.AssetType?.Name,
                ConnectionState.PhysicalState.Name,
                _masterData.ScheduledMeterReadingDate?.MonthAndDay,
                _masterData.Capacity?.Kw,
                _exchangeGridAreas?.SourceGridArea.Value,
                _exchangeGridAreas?.TargetGridArea.Value);

            AddDomainEvent(@event);
        }

        private void RaiseMasterDataWasUpdatedEvent()
        {
            AddDomainEvent(new MasterDataWasUpdated(
                _masterData.Address.StreetName,
                _masterData.Address.StreetCode,
                _masterData.Address.City,
                _masterData.Address.Floor,
                _masterData.Address.Room,
                _masterData.Address.BuildingNumber,
                _masterData.Address.CountryCode?.Name,
                _masterData.Address.IsActual,
                _masterData.Address.LocationDescription,
                _masterData.Address.MunicipalityCode,
                _masterData.Address.PostCode,
                _masterData.Address.CitySubDivision,
                _masterData.Address.GeoInfoReference,
                _masterData.Capacity?.Kw,
                _masterData.AssetType?.Name,
                _masterData.ConnectionType?.Name,
                _masterData.DisconnectionType?.Name,
                _masterData.EffectiveDate?.DateInUtc.ToString(),
                _masterData.MeteringConfiguration.Meter.Value,
                _masterData.MeteringConfiguration.Method.Name,
                _masterData.PowerLimit?.Ampere,
                _masterData.PowerLimit?.Kwh,
                _masterData.ProductionObligation,
                _masterData.ProductType.Name,
                _masterData.ReadingOccurrence.Name,
                _masterData.SettlementMethod?.Name,
                _masterData.UnitType.Name,
                _masterData.NetSettlementGroup?.Name,
                _masterData.PowerPlantGsrnNumber?.Value,
                _masterData.ScheduledMeterReadingDate?.MonthAndDay));
        }
    }
}
