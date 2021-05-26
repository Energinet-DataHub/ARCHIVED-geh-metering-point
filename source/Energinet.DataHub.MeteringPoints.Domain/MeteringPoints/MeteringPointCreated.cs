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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public class MeteringPointCreated : DomainEventBase
    {
        public MeteringPointCreated(MeteringPointId meteringPointId, GsrnNumber gsrnNumber, MeteringPointType meteringPointType, GridAreaId gridAreaId)
        {
            MeteringPointId = meteringPointId;
            GsrnNumber = gsrnNumber;
            MeteringPointType = meteringPointType;
            GridAreaId = gridAreaId;
        }

        public MeteringPointId MeteringPointId { get; }

        public GsrnNumber GsrnNumber { get; }

        public MeteringPointType MeteringPointType { get; }

        public GridAreaId GridAreaId { get; }
    }
}
