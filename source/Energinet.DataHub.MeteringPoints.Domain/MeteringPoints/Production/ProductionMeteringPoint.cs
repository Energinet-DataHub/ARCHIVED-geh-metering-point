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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules.Connect;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Production.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Production
{
    public class ProductionMeteringPoint : MarketMeteringPoint
    {
        private bool _productionObligation;
        private MasterData _masterData;

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind Address in main constructor
        private ProductionMeteringPoint() { }
#pragma warning restore 8618

        private ProductionMeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            MeteringPointType meteringPointType,
            GridAreaLinkId gridAreaLinkId,
            EffectiveDate effectiveDate,
            bool productionObligation,
            DisconnectionType disconnectionType,
            ConnectionType? connectionType,
            MasterData masterData)
            : base(
                id,
                gsrnNumber,
                meteringPointType,
                gridAreaLinkId,
                effectiveDate,
                connectionType,
                disconnectionType,
                masterData)
        {
            _productionObligation = productionObligation;
            _masterData = masterData;
            ProductionObligation = false;
            ConnectionState = ConnectionState.New();

            var @event = new ProductionMeteringPointCreated(
                id.Value,
                GsrnNumber.Value,
                gridAreaLinkId.Value,
                MeteringConfiguration.Method.Name,
                _masterData.ProductType.Name,
                _masterData.ReadingOccurrence.Name,
                _masterData.UnitType.Name,
                _masterData.NetSettlementGroup?.Name!,
                _masterData.Address.City!,
                _masterData.Address.Floor!,
                _masterData.Address.Room!,
                _masterData.Address.BuildingNumber!,
                _masterData.Address.CountryCode?.Name!,
                _masterData.Address.MunicipalityCode!,
                _masterData.Address.PostCode!,
                _masterData.Address.StreetCode!,
                _masterData.Address.StreetName!,
                _masterData.Address.CitySubDivision!,
                _masterData.Address.IsActual.GetValueOrDefault(),
                _masterData.Address.GeoInfoReference,
                _masterData.PowerPlantGsrnNumber!.Value,
                MeteringConfiguration.Meter?.Value!,
                _masterData.Address.LocationDescription!,
                _masterData.PowerLimit.Ampere,
                _masterData.PowerLimit.Kwh,
                effectiveDate.DateInUtc,
                DisconnectionType.Name,
                ConnectionType?.Name!,
                _masterData.AssetType?.Name!,
                ConnectionState.PhysicalState.Name,
                ProductionObligation,
                _masterData.Capacity!.Kw);

            AddDomainEvent(@event);
        }

        protected bool ProductionObligation { get; }

        public static new BusinessRulesValidationResult CanCreate(ProductionMeteringPointDetails meteringPointDetails)
        {
            if (meteringPointDetails == null) throw new ArgumentNullException(nameof(meteringPointDetails));
            var generalRuleCheckResult = MarketMeteringPoint.CanCreate(meteringPointDetails);
            var rules = new List<IBusinessRule>()
            {
                new CapacityRequirementRule(meteringPointDetails.Capacity, meteringPointDetails.NetSettlementGroup),
                new AssetTypeRequirementRule(meteringPointDetails.AssetType),
                new PowerplantRequirementRule(meteringPointDetails.GsrnNumber, meteringPointDetails.PowerPlantGsrnNumber),
            };

            return new BusinessRulesValidationResult(generalRuleCheckResult.Errors.Concat(rules.Where(r => r.IsBroken).Select(r => r.ValidationError).ToList()));
        }

        public static ProductionMeteringPoint Create(ProductionMeteringPointDetails meteringPointDetails, MasterData masterData)
        {
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));
            if (!CanCreate(meteringPointDetails).Success)
            {
                throw new ProductionMeteringPointException($"Cannot create production metering point due to violation of one or more business rules.");
            }

            return new ProductionMeteringPoint(
                meteringPointDetails.Id,
                meteringPointDetails.GsrnNumber,
                MeteringPointType.Production,
                meteringPointDetails.GridAreaLinkId,
                meteringPointDetails.EffectiveDate,
                false,
                meteringPointDetails.DisconnectionType,
                meteringPointDetails.ConnectionType,
                masterData);
        }

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
    }
}
