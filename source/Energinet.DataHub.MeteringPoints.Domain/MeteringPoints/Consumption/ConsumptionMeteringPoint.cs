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
using System.Collections.ObjectModel;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules.Connect;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption
{
    #pragma warning disable
    public class ConsumptionMeteringPoint : MarketMeteringPoint
    {
        private SettlementMethod _settlementMethod;
        private NetSettlementGroup _netSettlementGroup;
        private DisconnectionType _disconnectionType;
        private ConnectionType _connectionType;
        private AssetType? _assetType;
        private bool _isAddressWashable;
        private ScheduledMeterReadingDate? _scheduledMeterReadingDate;

        private ConsumptionMeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            Address address,
            bool isAddressWashable,
            MeteringPointSubType meteringPointSubType,
            MeteringPointType meteringPointType,
            GridAreaId gridAreaId,
            GsrnNumber? powerPlantGsrnNumber,
            string? locationDescription,
            MeasurementUnitType unitType,
            string? meterNumber,
            ReadingOccurrence meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            EffectiveDate effectiveDate,
            SettlementMethod settlementMethod,
            NetSettlementGroup netSettlementGroup,
            DisconnectionType disconnectionType,
            ConnectionType connectionType,
            AssetType? assetType,
            ScheduledMeterReadingDate? scheduledMeterReadingDate)
            : base(
                id,
                gsrnNumber,
                address,
                meteringPointSubType,
                meteringPointType,
                gridAreaId,
                powerPlantGsrnNumber,
                locationDescription,
                unitType,
                meterNumber,
                meterReadingOccurrence,
                maximumCurrent,
                maximumPower,
                effectiveDate)
        {
            _settlementMethod = settlementMethod;
            _netSettlementGroup = netSettlementGroup;
            _disconnectionType = disconnectionType;
            _connectionType = connectionType;
            _assetType = assetType;
            _productType = ProductType.EnergyActive;
            _isAddressWashable = isAddressWashable;
            ConnectionState = ConnectionState.New();
            _scheduledMeterReadingDate = scheduledMeterReadingDate;

            var @event = new ConsumptionMeteringPointCreated(
                id.Value,
                GsrnNumber.Value,
                gridAreaId.Value,
                meteringPointSubType.Name,
                _productType.Name,
                meterReadingOccurrence.Name,
                unitType.Name,
                _settlementMethod.Name,
                netSettlementGroup.Name,
                address.City,
                address.Floor,
                address.Room,
                address.BuildingNumber,
                address.CountryCode,
                address.MunicipalityCode,
                address.PostCode,
                address.StreetCode,
                address.StreetName,
                address.CitySubDivision,
                isAddressWashable,
                powerPlantGsrnNumber.Value,
                locationDescription,
                meterNumber,
                maximumCurrent,
                maximumPower,
                effectiveDate.DateInUtc,
                _disconnectionType.Name,
                _connectionType.Name,
                _assetType.Name,
                ConnectionState.PhysicalState.Name,
                _scheduledMeterReadingDate?.MonthAndDay);

            AddDomainEvent(@event);
        }

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind Address in main constructor
        private ConsumptionMeteringPoint() { }
#pragma warning restore 8618

        public override BusinessRulesValidationResult ConnectAcceptable(ConnectionDetails connectionDetails)
        {
            var rules = new Collection<IBusinessRule>
            {
                new MeteringPointMustHavePhysicalStateNewRule(GsrnNumber, _meteringPointType, ConnectionState.PhysicalState),
                new MustHaveEnergySupplierRule(GsrnNumber, connectionDetails, EnergySupplierDetails),
            };

            return new BusinessRulesValidationResult(rules);
        }

        public override void Connect(ConnectionDetails connectionDetails)
        {
            if (connectionDetails == null) throw new ArgumentNullException(nameof(connectionDetails));
            if (!ConnectAcceptable(connectionDetails).Success)
            {
                throw MeteringPointConnectException.Create(Id, GsrnNumber);
            }

            ConnectionState = ConnectionState.Connected(connectionDetails.EffectiveDate);
            AddDomainEvent(new MeteringPointConnected(Id.Value, GsrnNumber.Value, connectionDetails.EffectiveDate));
        }

        public static BusinessRulesValidationResult CanCreate(MeteringPointDetails meteringPointDetails)
        {
            var rules = new Collection<IBusinessRule>()
            {
                new PowerPlantIsRequiredForNetSettlementGroupRule(meteringPointDetails.GsrnNumber, meteringPointDetails.NetSettlementGroup, meteringPointDetails.PowerPlantGsrnNumber),
                new StreetNameIsRequiredRule(meteringPointDetails.GsrnNumber, meteringPointDetails.Address),
                new PostCodeIsRequiredRule(meteringPointDetails.Address),
                new CityIsRequiredRule(meteringPointDetails.Address),
                new ScheduledMeterReadingDateRule(meteringPointDetails.ScheduledMeterReadingDate, meteringPointDetails.NetSettlementGroup),
            };

            return new BusinessRulesValidationResult(rules);
        }

        public static ConsumptionMeteringPoint Create(MeteringPointDetails meteringPointDetails)
        {
            if (!CanCreate(meteringPointDetails).Success)
            {
                throw new ConsumptionMeteringPointException($"Cannot create consumption metering point due to violation of one or more business rules.");
            }
            return new ConsumptionMeteringPoint(
                meteringPointDetails.Id,
                meteringPointDetails.GsrnNumber,
                meteringPointDetails.Address,
                meteringPointDetails.IsAddressWashable,
                meteringPointDetails.MeteringPointSubType,
                MeteringPointType.Consumption,
                meteringPointDetails.GridAreaId,
                meteringPointDetails.PowerPlantGsrnNumber,
                meteringPointDetails.LocationDescription,
                meteringPointDetails.UnitType,
                meteringPointDetails.MeterNumber,
                meteringPointDetails.ReadingOccurrence,
                meteringPointDetails.MaximumCurrent,
                meteringPointDetails.MaximumPower,
                meteringPointDetails.EffectiveDate,
                meteringPointDetails.SettlementMethod,
                meteringPointDetails.NetSettlementGroup,
                meteringPointDetails.DisconnectionType,
                meteringPointDetails.ConnectionType,
                meteringPointDetails.AssetType,
                meteringPointDetails.ScheduledMeterReadingDate);
        }
    }
}
