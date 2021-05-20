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
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public class ProductionMeteringPoint : MeteringPoint
    {
        public ProductionMeteringPoint(
            GsrnNumber gsrnNumber,
            string streetName,
            string postCode,
            string cityName,
            string countryCode,
            bool isAddressWashable,
            string physicalStatusOfMeteringPoint,
            string meteringPointSubType,
            string typeOfMeteringPoint,
            string meteringGridArea,
            string powerPlant,
            string locationDescription,
            string productType,
            string parentRelatedMeteringPoint,
            string unitType,
            string meterNumber,
            Instant? meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            Instant? occurenceDate,
            int productionObligation,
            int netSettlementGroup,
            int disconnectionType,
            int connectionType)
            : base(
                gsrnNumber,
                streetName,
                postCode,
                cityName,
                countryCode,
                isAddressWashable,
                physicalStatusOfMeteringPoint,
                meteringPointSubType,
                typeOfMeteringPoint,
                meteringGridArea,
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

        public ProductionMeteringPoint(
            GsrnNumber gsrnNumber,
            Guid id,
            string streetName,
            string postCode,
            string cityName,
            string countryCode,
            bool isAddressWashable,
            string physicalStatusOfMeteringPoint,
            string meteringPointSubType,
            string typeOfMeteringPoint,
            string meteringGridArea,
            string powerPlant,
            string locationDescription,
            string productType,
            string parentRelatedMeteringPoint,
            string unitType,
            string meterNumber,
            Instant? meterReadingOccurence,
            int maximumCurrent,
            int maximumPower,
            Instant? occurenceDate,
            int productionObligation,
            int netSettlementGroup,
            int disconnectionType,
            int connectionType)
            : base(
                gsrnNumber,
                id,
                streetName,
                postCode,
                cityName,
                countryCode,
                isAddressWashable,
                physicalStatusOfMeteringPoint,
                meteringPointSubType,
                typeOfMeteringPoint,
                meteringGridArea,
                powerPlant,
                locationDescription,
                productType,
                parentRelatedMeteringPoint,
                unitType,
                meterNumber,
                meterReadingOccurence,
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

        public int DisconnectionType { get; }

        public int ConnectionType { get; }
    }
}
