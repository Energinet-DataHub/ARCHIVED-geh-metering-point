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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Exchange;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Execeptions;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules.Connect;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public class MeteringPoint : AggregateRootBase
    {
        private readonly ExchangeDetails? _exchangeDetails;
        private readonly EffectiveDate _effectiveDate;
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
            EffectiveDate effectiveDate,
            MasterData masterData)
        {
            Id = id;
            GsrnNumber = gsrnNumber;
            MeteringPointType = meteringPointType;
            GridAreaLinkId = gridAreaLinkId;
            _effectiveDate = effectiveDate;
            _masterData = masterData;

            RaiseMeteringPointCreated();
        }

        private MeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            GridAreaLinkId gridAreaLinkId,
            EffectiveDate effectiveDate,
            ExchangeDetails exchangeDetails,
            MasterData masterData)
        {
            Id = id;
            GsrnNumber = gsrnNumber;
            MeteringPointType = MeteringPointType.Exchange;
            GridAreaLinkId = gridAreaLinkId;
            _effectiveDate = effectiveDate;
            _exchangeDetails = exchangeDetails ?? throw new ArgumentNullException(nameof(exchangeDetails));
            _masterData = masterData;

            RaiseMeteringPointCreated();
        }

        public MeteringPointId Id { get; }

        public GsrnNumber GsrnNumber { get; }

        public Address Address => _masterData.Address;

        internal GridAreaLinkId GridAreaLinkId { get; }

        internal MeteringPointType MeteringPointType { get; }

        internal MeteringConfiguration MeteringConfiguration => _masterData.MeteringConfiguration;

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
            EffectiveDate effectiveDate,
            ExchangeDetails exchangeDetails,
            MasterData masterData)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (gsrnNumber == null) throw new ArgumentNullException(nameof(gsrnNumber));
            if (gridAreaLinkId == null) throw new ArgumentNullException(nameof(gridAreaLinkId));
            if (effectiveDate == null) throw new ArgumentNullException(nameof(effectiveDate));
            if (exchangeDetails == null) throw new ArgumentNullException(nameof(exchangeDetails));
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));

            return new MeteringPoint(id, gsrnNumber, gridAreaLinkId, effectiveDate, exchangeDetails, masterData);
        }

        public static MeteringPoint Create(MeteringPointId id, GsrnNumber gsrnNumber, MeteringPointType meteringPointType, GridAreaLinkId gridAreaLinkId, EffectiveDate effectiveDate, MasterData masterData)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (gsrnNumber == null) throw new ArgumentNullException(nameof(gsrnNumber));
            if (meteringPointType is null) throw new ArgumentNullException(nameof(meteringPointType));
            if (gridAreaLinkId == null) throw new ArgumentNullException(nameof(gridAreaLinkId));
            if (effectiveDate == null) throw new ArgumentNullException(nameof(effectiveDate));
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));
            return new MeteringPoint(id, gsrnNumber, meteringPointType, gridAreaLinkId, effectiveDate, masterData);
        }

        public BusinessRulesValidationResult CanChangeAddress(Address address)
        {
            var canBeChangedCheck = CanBeChanged();
            if (canBeChangedCheck.Success == false)
            {
                return canBeChangedCheck;
            }

            var rules = new List<IBusinessRule>()
            {
                new StreetNameIsRequiredRule(GsrnNumber, address),
                new PostCodeIsRequiredRule(address),
                new CityIsRequiredRule(address),
            };
            return new BusinessRulesValidationResult(rules);
        }

        public void ChangeAddress(Address newAddress)
        {
            if (newAddress == null) throw new ArgumentNullException(nameof(newAddress));
            ThrowIfClosedDown();
            var checkResult = CanChangeAddress(newAddress);
            if (checkResult.Success == false)
            {
                throw new MasterDataChangeException(checkResult.Errors.ToList());
            }

            if (newAddress.Equals(Address) == false)
            {
                var builder =
                    new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(MeteringPointType))
                        .WithNetSettlementGroup(_masterData.NetSettlementGroup?.Name)
                        .WithCapacity(_masterData.Capacity?.Kw)
                        .WithMeteringConfiguration(_masterData.MeteringConfiguration.Method.Name, _masterData.MeteringConfiguration.Meter.Value)
                        .WithReadingPeriodicity(_masterData.ReadingOccurrence.Name)
                        .WithScheduledMeterReadingDate(_masterData.ScheduledMeterReadingDate?.MonthAndDay)
                        .WithAddress(
                            newAddress.StreetName,
                            newAddress.StreetCode,
                            newAddress.BuildingNumber,
                            newAddress.City,
                            newAddress.CitySubDivision,
                            newAddress.PostCode,
                            newAddress.CountryCode,
                            newAddress.Floor,
                            newAddress.Room,
                            newAddress.MunicipalityCode,
                            newAddress.IsActual,
                            newAddress.GeoInfoReference,
                            newAddress.LocationDescription)
                        .WithAssetType(_masterData.AssetType?.Name)
                        .WithConnectionType(_masterData.ConnectionType?.Name)
                        .WithDisconnectionType(_masterData.DisconnectionType?.Name)
                        .WithPowerLimit(_masterData.PowerLimit.Kwh, _masterData.PowerLimit.Ampere)
                        .WithPowerPlant(_masterData.PowerPlantGsrnNumber?.Value)
                        .WithProductType(_masterData.ProductType.Name)
                        .WithSettlementMethod(_masterData.SettlementMethod?.Name)
                        .WithMeasurementUnitType(_masterData.UnitType?.Name)
                        .EffectiveOn(_masterData.EffectiveDate?.ToString());

                _masterData = builder.Build();

                AddDomainEvent(new AddressChanged(
                    Address.StreetName,
                    Address.PostCode,
                    Address.City,
                    Address.StreetCode,
                    Address.BuildingNumber,
                    Address.CitySubDivision,
                    Address.CountryCode?.Name,
                    Address.Floor,
                    Address.Room,
                    Address.MunicipalityCode.GetValueOrDefault(),
                    Address.IsActual.GetValueOrDefault(),
                    Address.GeoInfoReference.GetValueOrDefault()));
            }
        }

        public void ChangeMeteringConfiguration(MeteringConfiguration configuration, EffectiveDate effectiveDate)
        {
            if (effectiveDate == null) throw new ArgumentNullException(nameof(effectiveDate));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            ThrowIfClosedDown();

            if (MeteringConfiguration.Equals(configuration))
            {
                return;
            }

            var builder =
                new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(MeteringPointType))
                    .WithNetSettlementGroup(_masterData.NetSettlementGroup?.Name)
                    .WithCapacity(_masterData.Capacity?.Kw)
                    .WithMeteringConfiguration(configuration.Method.Name, configuration.Meter.Value)
                    .WithReadingPeriodicity(_masterData.ReadingOccurrence.Name)
                    .WithScheduledMeterReadingDate(_masterData.ScheduledMeterReadingDate?.MonthAndDay)
                    .WithAddress(
                        Address.StreetName,
                        Address.StreetCode,
                        Address.BuildingNumber,
                        Address.City,
                        Address.CitySubDivision,
                        Address.PostCode,
                        Address.CountryCode,
                        Address.Floor,
                        Address.Room,
                        Address.MunicipalityCode,
                        Address.IsActual,
                        Address.GeoInfoReference,
                        Address.LocationDescription)
                    .WithAssetType(_masterData.AssetType?.Name)
                    .WithConnectionType(_masterData.ConnectionType?.Name)
                    .WithDisconnectionType(_masterData.DisconnectionType?.Name)
                    .WithPowerLimit(_masterData.PowerLimit.Kwh, _masterData.PowerLimit.Ampere)
                    .WithPowerPlant(_masterData.PowerPlantGsrnNumber?.Value)
                    .WithProductType(_masterData.ProductType.Name)
                    .WithSettlementMethod(_masterData.SettlementMethod?.Name)
                    .WithMeasurementUnitType(_masterData.UnitType?.Name);

            _masterData = builder.Build();

            AddDomainEvent(new MeteringConfigurationChanged(
                Id.Value.ToString(),
                GsrnNumber.Value,
                MeteringConfiguration.Meter.Value,
                MeteringConfiguration.Method.Name,
                effectiveDate.ToString()));
        }

        #pragma warning disable CA1024 // Use properties where appropriate
        public MeteringConfiguration GetMeteringConfiguration()
        {
            return MeteringConfiguration;
        }
        #pragma warning restore

        public BusinessRulesValidationResult CanBeChanged()
        {
            var errors = new List<ValidationError>();
            if (ConnectionState.PhysicalState == PhysicalState.ClosedDown)
            {
                errors.Add(new ClosedDownMeteringPointCannotBeChangedError());
            }

            return new BusinessRulesValidationResult(errors);
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
                throw new CannotAssignEnergySupplierExeception();
            }

            EnergySupplierDetails = energySupplierDetails;
            AddDomainEvent(new EnergySupplierDetailsChanged(Id.Value, EnergySupplierDetails.StartOfSupply));
        }

        public virtual BusinessRulesValidationResult ConnectAcceptable(ConnectionDetails connectionDetails)
        {
            var rules = new Collection<IBusinessRule>
            {
                new MeteringPointMustHavePhysicalStateNewRule(GsrnNumber, MeteringPointType, ConnectionState.PhysicalState),
                new MustHaveEnergySupplierRule(GsrnNumber, connectionDetails, EnergySupplierDetails),
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

        internal void SetParent(MeteringPointId? parentId)
        {
            if (parentId is not null)
            {
                _parentMeteringPoint = parentId;
                AddDomainEvent(new CoupledToParent(Id.Value, parentId.Value));
            }
        }

        private void ThrowIfClosedDown()
        {
            var checkResult = CanBeChanged();
            if (checkResult.Success == false)
            {
                throw new MeteringPointClosedForChangesException(checkResult.Errors);
            }
        }

        private void RaiseMeteringPointCreated()
        {
            var @event = new Events.MeteringPointCreated(
                MeteringPointType.Name,
                Id.Value,
                GsrnNumber.Value,
                GridAreaLinkId.Value,
                MeteringConfiguration.Method.Name,
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
                MeteringConfiguration.Meter?.Value,
                _masterData.PowerLimit.Ampere,
                _masterData.PowerLimit.Kwh,
                _effectiveDate.DateInUtc,
                _masterData.DisconnectionType?.Name!,
                _masterData.ConnectionType?.Name,
                _masterData.AssetType?.Name,
                ConnectionState.PhysicalState.Name,
                _masterData.ScheduledMeterReadingDate?.MonthAndDay,
                _masterData.Capacity?.Kw,
                _exchangeDetails?.SourceGridArea.Value,
                _exchangeDetails?.TargetGridArea.Value);

            AddDomainEvent(@event);
        }
    }
}
