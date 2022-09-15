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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.MeteringPoints.ActorRegistrySync.Entities;
using Microsoft.Data.SqlClient;

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

    public async Task CleanUpAsync()
    {
        if (_transaction == null) await BeginTransactionAsync().ConfigureAwait(false);
        await _sqlConnection.ExecuteAsync("DELETE FROM [dbo].[GridAreaLinks]", transaction: _transaction)
            .ConfigureAwait(false);
        await _sqlConnection.ExecuteAsync("DELETE FROM [dbo].[GridAreas]", transaction: _transaction)
            .ConfigureAwait(false);
        await _sqlConnection.ExecuteAsync("DELETE FROM [dbo].[UserActor]", transaction: _transaction)
            .ConfigureAwait(false);
        await _sqlConnection.ExecuteAsync("DELETE FROM [dbo].[Actor]", transaction: _transaction)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<UserActor>> GetUserActorsAsync()
    {
        return await _sqlConnection.QueryAsync<UserActor>(
            @"SELECT UserId, ActorId
                   FROM [dbo].[UserActor]").ConfigureAwait(false) ?? (IEnumerable<UserActor>)Array.Empty<object>();
    }

    public async Task InsertGriAreaLinkAsync(IEnumerable<GridAreaLink> gridAreaLinks)
    {
        if (gridAreaLinks == null) throw new ArgumentNullException(nameof(gridAreaLinks));

        if (_transaction == null) await BeginTransactionAsync().ConfigureAwait(false);

        var stringBuilder = new StringBuilder();
        foreach (var gridAreaLink in gridAreaLinks)
        {
            stringBuilder.Append("INSERT INTO [dbo].[GridAreaLinks] ([Id],[GridAreaId]) VALUES ('" + gridAreaLink.GridLinkId + "', '" + gridAreaLink.GridAreaId + "')");
            stringBuilder.AppendLine();
        }

        await _sqlConnection.ExecuteAsync(
            stringBuilder.ToString(),
            transaction: _transaction).ConfigureAwait(false);
    }

    public async Task InsertActorsAsync(IEnumerable<ActorRegistryActor> actors)
    {
        if (actors == null) throw new ArgumentNullException(nameof(actors));

        if (_transaction == null) await BeginTransactionAsync().ConfigureAwait(false);

        var stringBuilder = new StringBuilder();
        foreach (var actor in actors)
        {
            stringBuilder.Append(@"INSERT INTO [dbo].[Actor] ([Id],[IdentificationNumber],[IdentificationType])
             VALUES ('" + actor.Id + "', '" + actor.IdentificationNumber + "', '" + actor.IdentificationType + "')");
            stringBuilder.AppendLine();
        }

        await _sqlConnection.ExecuteAsync(
            stringBuilder.ToString(),
            transaction: _transaction).ConfigureAwait(false);
    }

    public async Task InsertGridAreasAsync(IEnumerable<GridArea> gridAreas)
    {
        if (gridAreas == null) throw new ArgumentNullException(nameof(gridAreas));

        if (_transaction == null) await BeginTransactionAsync().ConfigureAwait(false);

        var stringBuilder = new StringBuilder();
        foreach (var gridArea in gridAreas)
        {
            stringBuilder.Append(
                @"INSERT INTO [dbo].[GridAreas]([Id],[Code],[Name],[ActorId])
                  VALUES ('" + gridArea.Id + "', '" + gridArea.Code + "', '" + gridArea.Name + "', '" + gridArea.ActorId + "')");
            stringBuilder.AppendLine();
        }

        await _sqlConnection.ExecuteAsync(
            stringBuilder.ToString(),
            transaction: _transaction).ConfigureAwait(false);
    }

    public async Task<int> InsertUserActorsAsync(IEnumerable<UserActor> userActors)
    {
        if (userActors == null) throw new ArgumentNullException(nameof(userActors));

        if (!userActors.Any()) return 0;

        if (_transaction == null) await BeginTransactionAsync().ConfigureAwait(false);
        var stringBuilder = new StringBuilder();

        foreach (var userActor in userActors)
        {
            stringBuilder.Append(@"INSERT INTO [dbo].[UserActor] (UserId, ActorId) VALUES ('" + userActor.UserId + "', '" + userActor.ActorId + "')");
            stringBuilder.AppendLine();
        }

        return await _sqlConnection.ExecuteAsync(
            stringBuilder.ToString(),
            transaction: _transaction).ConfigureAwait(false);
    }

    public async Task<int> InsertUsersAsync(IReadOnlyCollection<Guid> userIds)
    {
        if (userIds == null) throw new ArgumentNullException(nameof(userIds));
        if (_transaction == null) await BeginTransactionAsync().ConfigureAwait(false);

        var stringBuilder = new StringBuilder();

        // The only protection against SQL injections here is that userId is that the value must be a Guid and that it is an internal request. But it'll have to work for now.
        foreach (var userId in userIds)
        {
            var query = @$"
                    BEGIN
                        IF NOT EXISTS (
                            SELECT Id FROM [dbo].[User]
                            WHERE Id = '{userId}'
                        )
                        BEGIN
                            INSERT INTO [dbo].[User] (Id)
                            VALUES ('{userId}')
                        END
                    END";

            stringBuilder.Append(query);
            stringBuilder.AppendLine();
        }

        var rowsAffected = await _sqlConnection.ExecuteAsync(
            stringBuilder.ToString(),
            transaction: _transaction).ConfigureAwait(false);

        // In case of no rows effected the value will be -1 so we set 0 as our lower bound
        return Math.Max(rowsAffected, 0);
    }

    public async Task<IEnumerable<UserActor>> GetUserActorsByUserIdsAsync(IReadOnlyCollection<Guid> userIds)
    {
        return await _sqlConnection.QueryAsync<UserActor>(
            @"SELECT UserId, ActorId
                FROM [dbo].[UserActor]
                WHERE UserId in @userIds",
            new { userIds }).ConfigureAwait(false);
    }

    public async Task<IEnumerable<MeteringPointActor>> GetActorIdsByGlnNumbersAsync(IReadOnlyCollection<string> glnNumbers)
    {
        return await _sqlConnection.QueryAsync<MeteringPointActor>(
            @"SELECT IdentificationNumber, IdentificationType, Roles, Id
                    FROM [dbo].[Actor]
                    WHERE Actor.IdentificationNumber IN @glnNumbers",
            new { glnNumbers }).ConfigureAwait(false);
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync().ConfigureAwait(false);
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
        await _sqlConnection.OpenAsync().ConfigureAwait(false);
        _transaction = await _sqlConnection.BeginTransactionAsync().ConfigureAwait(false);
    }
}
