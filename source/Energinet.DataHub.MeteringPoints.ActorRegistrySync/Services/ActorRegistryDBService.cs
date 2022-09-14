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

    public SqlConnection SqlConnection => _sqlConnection;

    public async Task<IEnumerable<GridAreaLink>> GetGriAreaLinkAsync()
    {
        return await _sqlConnection.QueryAsync<GridAreaLink>(
            @$"SELECT [Id] AS {nameof(GridAreaLink.GridLinkId)},
                       [GridAreaId] AS {nameof(GridAreaLink.GridAreaId)}
                   FROM [dbo].[GridAreaLinkNew]").ConfigureAwait(false) ?? (IEnumerable<GridAreaLink>)Array.Empty<object>();
    }

    public async Task<IEnumerable<GridArea>> GetGridAreasAsync()
    {
        var sqlStatement = @$"SELECT ga.Id AS {nameof(GridArea.Id)},
                            ga.Code AS {nameof(GridArea.Code)},
	                        ga.Name AS {nameof(GridArea.Name)},
	                        a.ActorId AS {nameof(GridArea.ActorId)}
                            FROM [dbo].[GridAreaNew] ga
                            JOIN [dbo].[GridAreaActorInfoLink] gal ON ga.Id = gal.GridAreaId
                            JOIN [dbo].[ActorInfoNew] a ON a.Id = gal.ActorInfoId
                            WHERE a.ActorId IS NOT NULL";

        return await _sqlConnection.QueryAsync<GridArea>(sqlStatement).ConfigureAwait(false) ?? (IEnumerable<GridArea>)Array.Empty<object>();
    }

    public async Task<IEnumerable<ActorRegistryActor>> GetActorsAsync()
    {
        return await _sqlConnection.QueryAsync<ActorRegistryActor>(
            @$"SELECT ActorNumber AS {nameof(ActorRegistryActor.IdentificationNumber)}
                       ,'GLN' AS {nameof(ActorRegistryActor.IdentificationType)}
                       ,[Id] {nameof(ActorRegistryActor.Id)}
        FROM [dbo].[Actor]").ConfigureAwait(false) ?? (IEnumerable<ActorRegistryActor>)Array.Empty<object>();
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
