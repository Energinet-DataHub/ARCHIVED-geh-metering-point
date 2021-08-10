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

using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public class ConsumptionMeteringPoint : MeteringPoint
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
            PhysicalState physicalState,
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
            Instant? occurenceDate,
            SettlementMethod settlementMethod,
            NetSettlementGroup netSettlementGroup,
            DisconnectionType disconnectionType,
            ConnectionType connectionType,
            AssetType? assetType,
            string? parentRelatedMeteringPoint,
            ProductType productType)
            : base(
                id,
                gsrnNumber,
                address,
                physicalState,
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
                occurenceDate,
                parentRelatedMeteringPoint)
        {
            _settlementMethod = settlementMethod;
            _netSettlementGroup = netSettlementGroup;
            _disconnectionType = disconnectionType;
            _connectionType = connectionType;
            _assetType = assetType;
            _productType = productType;
            _isAddressWashable = isAddressWashable;
        }

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind Address in main constructor
        private ConsumptionMeteringPoint() { }
#pragma warning restore 8618

        public static BusinessRulesValidationResult CanCreate(GsrnNumber meteringPointGSRN, NetSettlementGroup netSettlementGroup, GsrnNumber? powerPlantGSRN)
        {
            var rules = new List<IBusinessRule>()
            {
                new PowerPlantIsRequiredForNetSettlementGroupRule(meteringPointGSRN, netSettlementGroup, powerPlantGSRN),
            };

            return new BusinessRulesValidationResult(rules);
        }
    }
}
