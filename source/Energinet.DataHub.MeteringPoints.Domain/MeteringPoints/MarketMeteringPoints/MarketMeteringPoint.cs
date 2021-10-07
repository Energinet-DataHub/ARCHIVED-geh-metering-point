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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Production;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints
{
    public abstract class MarketMeteringPoint : MeteringPoint
    {
        #pragma warning disable CS8618 // Ignore uninitialized properties
        protected MarketMeteringPoint()
        {
        }
        #pragma warning restore

        protected MarketMeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            Address address,
            MeteringMethod meteringMethod,
            MeteringPointType meteringPointType,
            GridAreaLinkId gridAreaLinkId,
            GsrnNumber? powerPlantGsrnNumber,
            LocationDescription? locationDescription,
            MeasurementUnitType unitType,
            MeterId? meterNumber,
            ReadingOccurrence meterReadingOccurrence,
            PowerLimit powerLimit,
            EffectiveDate effectiveDate,
            Capacity? capacity,
            ConnectionType? connectionType,
            DisconnectionType disconnectionType,
            NetSettlementGroup netSettlementGroup)
            : base(
                id,
                gsrnNumber,
                address,
                meteringMethod,
                meteringPointType,
                gridAreaLinkId,
                powerPlantGsrnNumber,
                locationDescription,
                unitType,
                meterNumber,
                meterReadingOccurrence,
                powerLimit,
                effectiveDate,
                capacity)
        {
            ConnectionType = connectionType;
            DisconnectionType = disconnectionType;
            NetSettlementGroup = netSettlementGroup;
        }

        protected EnergySupplierDetails? EnergySupplierDetails { get; private set; }

        protected ConnectionType? ConnectionType { get; private set; }

        protected DisconnectionType DisconnectionType { get; private set; }

        protected NetSettlementGroup NetSettlementGroup { get; private set; }

#pragma warning disable 108,114
        public static BusinessRulesValidationResult CanCreate(MeteringPointDetails meteringPointDetails)
#pragma warning restore 108,114
        {
            if (meteringPointDetails == null) throw new ArgumentNullException(nameof(meteringPointDetails));
            var generalRuleCheckResult = MeteringPoint.CanCreate(meteringPointDetails);
            var rules = new List<IBusinessRule>()
            {
                new MeterReadingOccurrenceRule(meteringPointDetails.ReadingOccurrence),
                new GeoInfoReferenceRequirementRule(meteringPointDetails.Address),
                new ConnectionTypeRequirementRule(meteringPointDetails.NetSettlementGroup, meteringPointDetails.ConnectionType),
                new MeteringMethodRule(meteringPointDetails.NetSettlementGroup, meteringPointDetails.MeteringMethod),
                new PostCodeIsRequiredRule(meteringPointDetails.Address),
                new CityIsRequiredRule(meteringPointDetails.Address),
                new StreetNameIsRequiredRule(meteringPointDetails.GsrnNumber, meteringPointDetails.Address),
            };

            return new BusinessRulesValidationResult(generalRuleCheckResult.Errors.Concat(rules.Where(r => r.IsBroken).Select(r => r.ValidationError).ToList()));
        }

        public void SetEnergySupplierDetails(EnergySupplierDetails energySupplierDetails)
        {
            if (energySupplierDetails == null) throw new ArgumentNullException(nameof(energySupplierDetails));
            if (EnergySupplierDetails! == energySupplierDetails) return;
            EnergySupplierDetails = energySupplierDetails;
            AddDomainEvent(new EnergySupplierDetailsChanged(Id.Value, EnergySupplierDetails.StartOfSupply));
        }
    }
}
