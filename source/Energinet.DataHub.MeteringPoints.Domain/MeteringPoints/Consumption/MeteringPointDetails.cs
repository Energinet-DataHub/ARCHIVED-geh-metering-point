﻿// Copyright 2020 Energinet DataHub A/S
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

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption
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
            PowerLimit PowerLimit,
            EffectiveDate EffectiveDate,
            SettlementMethod SettlementMethod,
            NetSettlementGroup NetSettlementGroup,
            DisconnectionType DisconnectionType,
            ConnectionType? ConnectionType,
            AssetType? AssetType,
            ScheduledMeterReadingDate? ScheduledMeterReadingDate,
            Capacity? Capacity)
        : MarketMeteringPoints.MeteringPointDetails(
            Id,
            GsrnNumber,
            Address,
            MeteringMethod,
            GridAreaLinkId,
            PowerPlantGsrnNumber,
            LocationDescription,
            MeterNumber,
            ReadingOccurrence,
            PowerLimit,
            EffectiveDate,
            SettlementMethod,
            NetSettlementGroup,
            DisconnectionType,
            ConnectionType,
            AssetType,
            ScheduledMeterReadingDate,
            Capacity);
}
