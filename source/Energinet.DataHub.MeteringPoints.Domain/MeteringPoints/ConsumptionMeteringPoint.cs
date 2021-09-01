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
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules.Connect;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
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

        public ConsumptionMeteringPoint(
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
            string meterNumber,
            ReadingOccurrence meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            EffectiveDate effectiveDate,
            SettlementMethod settlementMethod,
            NetSettlementGroup netSettlementGroup,
            DisconnectionType disconnectionType,
            ConnectionType connectionType,
            AssetType? assetType,
            ProductType productType)
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
                effectiveDate,
                null)
        {
            _settlementMethod = settlementMethod;
            _netSettlementGroup = netSettlementGroup;
            _disconnectionType = disconnectionType;
            _connectionType = connectionType;
            _assetType = assetType;
            _productType = productType;
            _isAddressWashable = isAddressWashable;
            ConnectionState = ConnectionState.New();

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
                ConnectionState.PhysicalState.Name);

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

        public static BusinessRulesValidationResult CanCreate(
            GsrnNumber meteringPointGSRN,
            NetSettlementGroup netSettlementGroup,
            GsrnNumber? powerPlantGSRN,
            Address address)
        {
            var rules = new Collection<IBusinessRule>()
            {
                new PowerPlantIsRequiredForNetSettlementGroupRule(meteringPointGSRN, netSettlementGroup, powerPlantGSRN),
                new StreetNameIsRequiredRule(meteringPointGSRN, address),
                new PostCodeIsRequiredRule(address),
                new CityIsRequiredRule(address),
            };

            return new BusinessRulesValidationResult(rules);
        }

        public static ConsumptionMeteringPoint Create(
            MeteringPointId id,
            GsrnNumber meteringPointGsrn,
            bool addressIsWashable,
            MeteringPointSubType meteringPointSubType,
            GridAreaId gridAreaId,
            NetSettlementGroup netSettlementGroup,
            GsrnNumber powerPlantGsrn,
            Address address,
            string locationDescription,
            MeasurementUnitType measurementUnitType,
            string meterNumber,
            ReadingOccurrence readingOccurrence,
            int maximumCurrent,
            int maximumPower,
            EffectiveDate effectiveDate,
            SettlementMethod settlementMethod,
            DisconnectionType disconnectionType,
            ConnectionType connectionType,
            AssetType assetType)
        {
            if (!CanCreate(meteringPointGsrn, netSettlementGroup, powerPlantGsrn, address).Success)
            {
                throw new ConsumptionMeteringPointException($"Cannot create consumption metering point due to violation of one or more business rules.");
            }
            return new ConsumptionMeteringPoint(
                id,
                meteringPointGsrn,
                address,
                addressIsWashable,
                meteringPointSubType,
                MeteringPointType.Consumption,
                gridAreaId,
                powerPlantGsrn,
                locationDescription,
                measurementUnitType,
                meterNumber,
                readingOccurrence,
                maximumCurrent,
                maximumPower,
                effectiveDate,
                settlementMethod,
                netSettlementGroup,
                disconnectionType,
                connectionType,
                assetType,
                ProductType.EnergyActive);
        }
    }
}
