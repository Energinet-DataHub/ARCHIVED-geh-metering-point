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
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Exchange
{
    public record ExchangeMeteringPointDetails(
        MeteringPointId Id,
        GsrnNumber GsrnNumber,
        Address Address,
        GridAreaLinkId GridAreaLinkId,
        LocationDescription? LocationDescription,
        ReadingOccurrence ReadingOccurrence,
        PowerLimit PowerLimit,
        EffectiveDate EffectiveDate,
        GridAreaLinkId FromGridLinkId,
        GridAreaLinkId ToGridLinkId,
        MeteringConfiguration MeteringConfiguration) : MeteringPointDetails(
        Id,
        GsrnNumber,
        Address,
        GridAreaLinkId,
        LocationDescription,
        MeteringConfiguration.Meter,
        ReadingOccurrence,
        PowerLimit,
        EffectiveDate,
        MeteringConfiguration);
}
