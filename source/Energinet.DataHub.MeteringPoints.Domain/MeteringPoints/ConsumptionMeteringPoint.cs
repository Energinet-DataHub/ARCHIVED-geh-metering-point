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
    public class ConsumptionMeteringPoint : MeteringPoint
    {
       public ConsumptionMeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            Address address,
            bool isAddressWashable,
            PhysicalState physicalState,
            MeteringPointSubType meteringPointSubType,
            MeteringPointType meteringPointType,
            GridAreaId gridAreaId,
            GsrnNumber powerPlant,
            string locationDescription,
            string parentRelatedMeteringPoint,
            string unitType,
            string meterNumber,
            ReadingOccurrence meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            Instant? occurenceDate,
            SettlementMethod settlementMethod,
            string netSettlementGroup,
            DisconnectionType disconnectionType,
            ConnectionType connectionType,
            AssetType assetType)
            : base(
                id,
                gsrnNumber,
                address,
                isAddressWashable,
                physicalState,
                meteringPointSubType,
                meteringPointType,
                gridAreaId,
                powerPlant,
                locationDescription,
                parentRelatedMeteringPoint,
                unitType,
                meterNumber,
                meterReadingOccurrence,
                maximumCurrent,
                maximumPower,
                occurenceDate)
        {
            SettlementMethod = settlementMethod;
            NetSettlementGroup = netSettlementGroup;
            DisconnectionType = disconnectionType;
            ConnectionType = connectionType;
            AssetType = assetType;
            ProductType = ProductType.EnergyActive;
        }

       #pragma warning disable CS8618 // Disable non-nullable check
       private ConsumptionMeteringPoint()
       {
           //EF core only
       }

       public SettlementMethod SettlementMethod { get; }

       public string NetSettlementGroup { get; }

       public DisconnectionType DisconnectionType { get; }

       public ConnectionType ConnectionType { get; }

       public AssetType AssetType { get; }
    }
}
