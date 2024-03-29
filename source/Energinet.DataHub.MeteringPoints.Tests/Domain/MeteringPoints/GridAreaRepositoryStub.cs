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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    public class GridAreaRepositoryStub : IGridAreaRepository
    {
        private readonly List<GridArea> _gridAreas = new();

        public void Add(GridArea gridArea)
        {
            _gridAreas.Add(gridArea);
        }

        public Task<GridArea?> GetByCodeAsync(GridAreaCode code)
        {
            return Task.FromResult(_gridAreas.FirstOrDefault(gridArea => gridArea.Code == code));
        }

        public Task<GridArea?> GetByLinkIdAsync(GridAreaLinkId linkId)
        {
            return Task.FromResult(_gridAreas.FirstOrDefault(gridArea => gridArea.DefaultLink.Id.Equals(linkId)));
        }
    }
}
