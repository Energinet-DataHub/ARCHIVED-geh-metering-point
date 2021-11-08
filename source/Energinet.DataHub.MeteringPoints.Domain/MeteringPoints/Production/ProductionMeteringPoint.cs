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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules.Connect;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Production.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Production
{
    #pragma warning disable
    public class ProductionMeteringPoint : MarketMeteringPoint
    {
        private NetSettlementGroup _netSettlementGroup;
        private AssetType? _assetType;
        private bool _productionObligation;

        private ProductionMeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            Address address,
            MeteringMethod meteringMethod,
            MeteringPointType meteringPointType,
            GridAreaLinkId gridAreaLinkId,
            GsrnNumber powerPlantGsrnNumber,
            LocationDescription? locationDescription,
            MeterId? meterNumber,
            ReadingOccurrence meterReadingOccurrence,
            PowerLimit? powerLimit,
            EffectiveDate effectiveDate,
            NetSettlementGroup netSettlementGroup,
            bool productionObligation,
            DisconnectionType disconnectionType,
            ConnectionType? connectionType,
            AssetType assetType,
            Capacity capacity)
            : base(
                id,
                gsrnNumber,
                address,
                meteringMethod,
                meteringPointType,
                gridAreaLinkId,
                powerPlantGsrnNumber,
                locationDescription,
                MeasurementUnitType.KWh,
                meterNumber,
                meterReadingOccurrence,
                powerLimit,
                effectiveDate,
                capacity,
                connectionType,
                disconnectionType,
                netSettlementGroup)
        {
            _netSettlementGroup = netSettlementGroup;
            _productionObligation = productionObligation;
            _assetType = assetType;
            _productType = ProductType.EnergyActive;
            ProductionObligation = false;
            ConnectionState = ConnectionState.New();

            var @event = new ProductionMeteringPointCreated(
                id.Value,
                GsrnNumber.Value,
                gridAreaLinkId.Value,
                meteringMethod.Name,
                _productType.Name,
                meterReadingOccurrence.Name,
                _unitType.Name,
                netSettlementGroup.Name,
                address.City,
                address.Floor,
                address.Room,
                address.BuildingNumber,
                address.CountryCode?.Name,
                address.MunicipalityCode,
                address.PostCode,
                address.StreetCode,
                address.StreetName,
                address.CitySubDivision,
                address.IsActual,
                address.GeoInfoReference,
                powerPlantGsrnNumber.Value,
                locationDescription.Value,
                meterNumber?.Value,
                powerLimit.Ampere,
                powerLimit.Kwh,
                effectiveDate.DateInUtc,
                DisconnectionType.Name,
                ConnectionType?.Name,
                _assetType.Name,
                ConnectionState.PhysicalState.Name,
                ProductionObligation,
                capacity.Kw);

            AddDomainEvent(@event);
        }

        protected bool ProductionObligation { get; }

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind Address in main constructor
        private ProductionMeteringPoint() { }
#pragma warning restore 8618

        public override BusinessRulesValidationResult ConnectAcceptable(ConnectionDetails connectionDetails)
        {
            var rules = new Collection<IBusinessRule>
            {
                new MeteringPointMustHavePhysicalStateNewRule(GsrnNumber, _meteringPointType, ConnectionState.PhysicalState),
                new MustHaveEnergySupplierRule(GsrnNumber, connectionDetails, GetCurrentEnergySupplier(connectionDetails.EffectiveDate)),
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

        public static BusinessRulesValidationResult CanCreate(ProductionMeteringPointDetails meteringPointDetails)
        {
            if (meteringPointDetails == null) throw new ArgumentNullException(nameof(meteringPointDetails));
            var generalRuleCheckResult= MarketMeteringPoint.CanCreate(meteringPointDetails);
            var rules = new List<IBusinessRule>()
            {
                new CapacityRequirementRule(meteringPointDetails.Capacity, meteringPointDetails.NetSettlementGroup),
                new AssetTypeRequirementRule(meteringPointDetails.AssetType),
                new PowerplantRequirementRule(meteringPointDetails.GsrnNumber, meteringPointDetails.PowerPlantGsrnNumber),
            };

            return new BusinessRulesValidationResult(generalRuleCheckResult.Errors.Concat(rules.Where(r => r.IsBroken).Select(r => r.ValidationError).ToList()));
        }

        public static ProductionMeteringPoint Create(ProductionMeteringPointDetails meteringPointDetails)
        {
            if (!CanCreate(meteringPointDetails).Success)
            {
                throw new ProductionMeteringPointException($"Cannot create production metering point due to violation of one or more business rules.");
            }
            return new ProductionMeteringPoint(
                meteringPointDetails.Id,
                meteringPointDetails.GsrnNumber,
                meteringPointDetails.Address,
                meteringPointDetails.MeteringMethod,
                MeteringPointType.Production,
                meteringPointDetails.GridAreaLinkId,
                meteringPointDetails.PowerPlantGsrnNumber,
                meteringPointDetails.LocationDescription,
                meteringPointDetails.MeterNumber,
                meteringPointDetails.ReadingOccurrence,
                meteringPointDetails.PowerLimit,
                meteringPointDetails.EffectiveDate,
                meteringPointDetails.NetSettlementGroup,
                false,
                meteringPointDetails.DisconnectionType,
                meteringPointDetails.ConnectionType,
                meteringPointDetails.AssetType,
                meteringPointDetails.Capacity);
        }
    }
}
