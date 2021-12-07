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

using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.MarketMeteringPoints
{
    public class MarketMeteringPointMock : MarketMeteringPoint
    {
        public MarketMeteringPointMock(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            Address address,
            MeteringPointType meteringPointType,
            GridAreaLinkId gridAreaLinkId,
            GsrnNumber? powerPlantGsrnNumber,
            PowerLimit powerLimit,
            EffectiveDate effectiveDate,
            Capacity? capacity,
            ConnectionType? connectionType,
            DisconnectionType disconnectionType,
            NetSettlementGroup netSettlementGroup,
            MeteringConfiguration meteringConfiguration,
            MasterData masterData)
            : base(
                id,
                gsrnNumber,
                address,
                meteringPointType,
                gridAreaLinkId,
                powerPlantGsrnNumber,
                powerLimit,
                effectiveDate,
                capacity,
                connectionType,
                disconnectionType,
                netSettlementGroup,
                meteringConfiguration,
                masterData)
        {
        }

        public override BusinessRulesValidationResult ConnectAcceptable(ConnectionDetails connectionDetails)
        {
            throw new System.NotImplementedException();
        }

        public override void Connect(ConnectionDetails connectionDetails)
        {
            throw new System.NotImplementedException();
        }
    }
}
