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
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.MeteringPoints.ActorRegistrySync.Entities;

namespace Energinet.DataHub.MeteringPoints.ActorRegistrySync.Services;

public class MeteringPointDbService : IDisposable
{
    private readonly SqlConnection _sqlConnection;
    private DbTransaction? _transaction;
    private bool _disposed;

    public MeteringPointDbService(string connectionString)
    {
        _sqlConnection = new SqlConnection(connectionString);
    }

    public SqlConnection SqlConnection => _sqlConnection;

    public async Task InsertGriAreaLinkAsync(IEnumerable<GridAreaLink> gridAreaLinks)
    {
        if (gridAreaLinks == null) throw new ArgumentNullException(nameof(gridAreaLinks));

        if (_transaction == null) await BeginTransactionAsync().ConfigureAwait(false);

        foreach (var gridAreaLink in gridAreaLinks)
        {
            await _sqlConnection.ExecuteAsync(
                "INSERT INTO [dbo].[GridAreaLinks] ([Id],[GridAreaId]) VALUES (@GridLinkId ,@GridAreaId)",
                new { gridAreaLink.GridLinkId, gridAreaLink.GridAreaId },
                _transaction).ConfigureAwait(false);
        }
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

    private async Task BeginTransactionAsync()
    {
        _transaction = await _sqlConnection.BeginTransactionAsync().ConfigureAwait(false);
    }
}
