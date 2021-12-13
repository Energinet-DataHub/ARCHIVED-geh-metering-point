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
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Special
{
    public record SpecialMeteringPointDetails(
        MeteringPointId Id,
        MeteringPointType MeteringPointType,
        GsrnNumber GsrnNumber,
        Address Address,
        GridAreaLinkId GridAreaLinkId,
        ReadingOccurrence ReadingOccurrence,
        PowerLimit PowerLimit,
        EffectiveDate EffectiveDate,
        GsrnNumber? PowerPlantGsrnNumber,
        Capacity? Capacity,
        AssetType? AssetType,
        MeteringConfiguration MeteringConfiguration,
        GsrnNumber? ParentRelatedMeteringPoint) : MeteringPointDetails(
        Id,
        GsrnNumber,
        Address,
        GridAreaLinkId,
        ReadingOccurrence,
        PowerLimit,
        EffectiveDate,
        MeteringConfiguration);
}
