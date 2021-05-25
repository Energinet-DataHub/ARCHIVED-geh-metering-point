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

using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public class ProductionMeteringPoint : MeteringPoint
    {
        public ProductionMeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            string streetName,
            string postCode,
            string cityName,
            string countryCode,
            bool isAddressWashable,
            PhysicalState physicalState,
            MeteringPointSubType meteringPointSubType,
            MeteringPointType meteringPointType,
            GridAreaId gridAreaId,
            string powerPlant,
            string locationDescription,
            string productType,
            string parentRelatedMeteringPoint,
            string unitType,
            string meterNumber,
            string meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            Instant? occurenceDate,
            int productionObligation,
            int netSettlementGroup,
            DisconnectionType disconnectionType,
            int connectionType)
            : base(
                id,
                gsrnNumber,
                streetName,
                postCode,
                cityName,
                countryCode,
                isAddressWashable,
                physicalState,
                meteringPointSubType,
                meteringPointType,
                gridAreaId,
                powerPlant,
                locationDescription,
                productType,
                parentRelatedMeteringPoint,
                unitType,
                meterNumber,
                meterReadingOccurrence,
                maximumCurrent,
                maximumPower,
                occurenceDate)
        {
            ProductionObligation = productionObligation;
            NetSettlementGroup = netSettlementGroup;
            DisconnectionType = disconnectionType;
            ConnectionType = connectionType;
        }

        public int ProductionObligation { get; }

        public int NetSettlementGroup { get; }

        public DisconnectionType DisconnectionType { get; }

        public int ConnectionType { get; }
    }
}
