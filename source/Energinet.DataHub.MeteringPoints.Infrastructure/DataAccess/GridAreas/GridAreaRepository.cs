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

using System;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.GridAreas
{
    public class GridAreaRepository : IGridAreaRepository
    {
        private readonly MeteringPointContext _meteringPointContext;

        public GridAreaRepository(MeteringPointContext meteringPointContext)
        {
            _meteringPointContext = meteringPointContext;
        }

        public void Add(GridArea gridArea)
        {
            if (gridArea is null) throw new ArgumentNullException(nameof(gridArea));

            _meteringPointContext.GridAreas.Add(gridArea);
        }

        public Task<GridArea?> GetByCodeAsync(GridAreaCode code)
        {
            return _meteringPointContext.GridAreas.SingleOrDefaultAsync(gridArea => gridArea.Code == code);
        }

        public Task<GridArea?> GetByLinkIdAsync(GridAreaLinkId linkId)
        {
            if (linkId == null) throw new ArgumentNullException(nameof(linkId));
            return _meteringPointContext.GridAreas
                .FromSqlInterpolated(
                    $"SELECT g.* FROM [dbo].[GridAreas] g JOIN [dbo].[GridAreaLinks] gl ON g.Id = gl.GridAreaId WHERE gl.Id = {linkId.Value}")
                .SingleOrDefaultAsync();
        }
    }
}
