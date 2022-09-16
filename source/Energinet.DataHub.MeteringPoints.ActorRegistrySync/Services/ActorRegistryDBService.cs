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
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.MeteringPoints.ActorRegistrySync.Entities;
using Microsoft.Data.SqlClient;

namespace Energinet.DataHub.MeteringPoints.ActorRegistrySync.Services;

public class ActorRegistryDbService : IDisposable
{
    private readonly SqlConnection _sqlConnection;
    private bool _disposed;

    public ActorRegistryDbService(string connectionString)
    {
        _sqlConnection = new SqlConnection(connectionString);
    }

    public async Task<IEnumerable<GridAreaLink>> GetGriAreaLinkAsync()
    {
        var sqlStatement = @$"
                    select
                        gl.Id {nameof(GridAreaLink.GridLinkId)},
                        gl.GridAreaId AS {nameof(GridAreaLink.GridAreaId)}
                    from GridAreaLinkNew gl
                    join GridAreaNew g on gl.GridAreaId = g.Id
                    join MarketRoleGridArea mrg on mrg.GridAreaId = g.Id
                    join MarketRole mr on mrg.MarketRoleId = mr.Id
                    where mr.[Function] = 14";

        return await _sqlConnection.QueryAsync<GridAreaLink>(sqlStatement).ConfigureAwait(false) ?? (IEnumerable<GridAreaLink>)Array.Empty<object>();
    }

    public async Task<IEnumerable<GridArea>> GetGridAreasAsync()
    {
        var sqlStatement = @$"
                    select
                        g.Code AS {nameof(GridArea.Code)},
                        g.Name AS {nameof(GridArea.Name)},
                        mr.ActorInfoId AS {nameof(GridArea.ActorId)},
                        g.Id AS {nameof(GridArea.Id)}
                    from GridAreaNew g
                    join MarketRoleGridArea mrg on mrg.GridAreaId = g.Id
                    join MarketRole mr on mrg.MarketRoleId = mr.Id
                    where mr.[Function] = 14";

        return await _sqlConnection.QueryAsync<GridArea>(sqlStatement).ConfigureAwait(false) ?? (IEnumerable<GridArea>)Array.Empty<object>();
    }

    public async Task<IEnumerable<ActorRegistryActor>> GetActorsAsync()
    {
        var sqlStatement = @$"
        select a.ActorNumber AS {nameof(ActorRegistryActor.IdentificationNumber)},
               'GLN' AS {nameof(ActorRegistryActor.IdentificationType)},
               (SELECT STRING_AGG([Function], ',') FROM MarketRole WHERE ActorInfoId = a.Id) AS {nameof(ActorRegistryActor.Roles)},
                a.ActorId AS {nameof(ActorRegistryActor.Id)}
        from [dbo].[ActorInfoNew] a where a.ActorId is not null";

        return await _sqlConnection.QueryAsync<ActorRegistryActor>(sqlStatement).ConfigureAwait(false) ?? (IEnumerable<ActorRegistryActor>)Array.Empty<object>();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _sqlConnection.Dispose();
        }

        _disposed = true;
    }
}
