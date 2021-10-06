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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public record MeteringPointDetails(
        MeteringPointId Id,
        GsrnNumber GsrnNumber,
        Address Address,
        MeteringMethod MeteringMethod,
        GridAreaLinkId GridAreaLinkId,
        GsrnNumber? PowerPlantGsrnNumber,
        LocationDescription? LocationDescription,
        MeterId? MeterNumber,
        ReadingOccurrence ReadingOccurrence,
        PowerLimit? PowerLimit,
        EffectiveDate EffectiveDate,
        NetSettlementGroup NetSettlementGroup,
        DisconnectionType DisconnectionType,
        ConnectionType? ConnectionType)
    {
        public MeteringPointDetails(MeteringPointId id, GsrnNumber gsrnNumber, Address address, MeteringMethod meteringMethod, GridAreaLinkId powerPlantGsrnNumber, LocationDescription locationDescription, MeterId meterNumber, ReadingOccurrence readingOccurrence, PowerLimit powerLimit, EffectiveDate effectiveDate, NetSettlementGroup netSettlementGroup, DisconnectionType disconnectionType, ConnectionType connectionType)
            : this(id, gsrnNumber, address, meteringMethod, powerPlantGsrnNumber, gsrnNumber, locationDescription, meterNumber, readingOccurrence, powerLimit, effectiveDate, netSettlementGroup, disconnectionType, connectionType)
        {
        }
    }
}
