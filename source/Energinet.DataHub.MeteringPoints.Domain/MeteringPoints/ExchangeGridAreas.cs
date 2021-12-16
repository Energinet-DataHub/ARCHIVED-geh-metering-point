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
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public class ExchangeGridAreas : ValueObject
    {
        private ExchangeGridAreas(GridAreaLinkId sourceGridArea, GridAreaLinkId targetGridArea)
        {
            SourceGridArea = sourceGridArea ?? throw new ArgumentNullException(nameof(sourceGridArea));
            TargetGridArea = targetGridArea ?? throw new ArgumentNullException(nameof(targetGridArea));
        }

        public GridAreaLinkId SourceGridArea { get; }

        public GridAreaLinkId TargetGridArea { get; }

        public static ExchangeGridAreas Create(GridAreaLinkId sourceGridArea, GridAreaLinkId targetGridArea)
        {
            return new ExchangeGridAreas(sourceGridArea, targetGridArea);
        }
    }
}
