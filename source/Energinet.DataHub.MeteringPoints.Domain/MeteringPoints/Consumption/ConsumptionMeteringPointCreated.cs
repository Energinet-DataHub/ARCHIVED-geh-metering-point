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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption
{
    public class ConsumptionMeteringPointCreated : DomainEventBase
    {
        public ConsumptionMeteringPointCreated(Guid meteringPointId, string gsrnNumber, Guid gridAreaId, string meteringPointSubType, string productType, string readingOccurrence, string unitType, string settlementMethod, string netSettlementGroup)
        {
            MeteringPointId = meteringPointId;
            GsrnNumber = gsrnNumber;
            GridAreaId = gridAreaId;
            MeteringPointSubType = meteringPointSubType;
            ProductType = productType;
            ReadingOccurrence = readingOccurrence;
            UnitType = unitType;
            SettlementMethod = settlementMethod;
            NetSettlementGroup = netSettlementGroup;
        }

        public Guid MeteringPointId { get; }

        public string GsrnNumber { get; }

        public Guid GridAreaId { get; }

        public string MeteringPointSubType { get; }

        public string ProductType { get; }

        public string ReadingOccurrence { get; }

        public string UnitType { get; }

        public string SettlementMethod { get; }

        public string NetSettlementGroup { get; }
    }
}
