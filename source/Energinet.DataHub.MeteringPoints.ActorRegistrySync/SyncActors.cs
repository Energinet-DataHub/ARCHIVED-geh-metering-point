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
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.MeteringPoints.ActorRegistrySync.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.ActorRegistrySync;

public static class SyncActors
{
    private static IEnumerable<UserActor>? _userActors;
    private static IEnumerable<Actor>? _actors;

    [FunctionName("SyncActors")]
    public static async Task RunAsync([TimerTrigger("%TIMER_TRIGGER%")] TimerInfo someTimer, ILogger log)
    {
        log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");
        await SyncActorsFromExternalSourceToDbAsync().ConfigureAwait(false);
    }

    private static async Task SyncActorsFromExternalSourceToDbAsync()
    {
        var actorRegistryConnectionString = Environment.GetEnvironmentVariable("ACTOR_REGISTRY_DB_CONNECTION_STRING");
        using var actorRegistrySqlConnection = new SqlConnection(actorRegistryConnectionString);
        var meteringPointConnectionString = Environment.GetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING");
        using var meteringPointSqlConnection = new SqlConnection(meteringPointConnectionString);
        await meteringPointSqlConnection.OpenAsync().ConfigureAwait(false);

        // Extract userActors before deleting tables
        _userActors = await GetUserActorsAsync(meteringPointSqlConnection).ConfigureAwait(false);

        using var transaction = meteringPointSqlConnection.BeginTransaction();
        await CleanUpAsync(meteringPointSqlConnection, transaction).ConfigureAwait(false);
        await SyncActorsAsync(actorRegistrySqlConnection, meteringPointSqlConnection, transaction).ConfigureAwait(false);
        await SyncGridAreasAsync(actorRegistrySqlConnection, meteringPointSqlConnection, transaction).ConfigureAwait(false);
        await SyncGridAreaLinksAsync(actorRegistrySqlConnection, meteringPointSqlConnection, transaction).ConfigureAwait(false);
        await SyncUserActorsAsync(meteringPointSqlConnection, transaction).ConfigureAwait(false);

        transaction.Commit();
    }

    private static async Task SyncUserActorsAsync(SqlConnection meteringPointSqlConnection, SqlTransaction transaction)
    {
        if (_userActors != null && _actors != null)
        {
            var userActorsToInsert = _userActors.Where(u => _actors.Any(actor => u.ActorId == actor.Id));
            foreach (var userActor in userActorsToInsert)
            {
                await meteringPointSqlConnection.ExecuteAsync(
                    "INSERT INTO [dbo].[UserActor] (UserId, ActorId) VALUES (@UserId, @ActorId)",
                    new { userActor.UserId, userActor.ActorId },
                    transaction).ConfigureAwait(false);
            }
        }
    }

    private static async Task CleanUpAsync(SqlConnection meteringPointSqlConnection, SqlTransaction transaction)
    {
        await meteringPointSqlConnection.ExecuteAsync("DELETE FROM [dbo].[GridAreaLinks]", transaction: transaction)
            .ConfigureAwait(false);
        await meteringPointSqlConnection.ExecuteAsync("DELETE FROM [dbo].[GridAreas]", transaction: transaction)
            .ConfigureAwait(false);
        await meteringPointSqlConnection.ExecuteAsync("DELETE FROM [dbo].[UserActor]", transaction: transaction)
            .ConfigureAwait(false);
        await meteringPointSqlConnection.ExecuteAsync("DELETE FROM [dbo].[Actor]", transaction: transaction)
            .ConfigureAwait(false);
    }

    private static string GetRoles(string actorRoles)
    {
        return string.Join(
            ',',
            actorRoles.Split(
                    ',',
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(MapRole));
    }

    private static string MapRole(string ediRole)
    {
        switch (ediRole)
        {
            case "DDK": return "BalanceResponsibleParty";
            case "DDM": return "GridAccessProvider";
            case "DDQ": return "BalancePowerSupplier";
            case "DDZ": return "MeteringPointAdministrator";
            case "EZ": return "SystemOperator";
            case "MDR": return "MeteredDataResponsible";
            default: throw new InvalidOperationException("Role not known: " + ediRole);
        }
    }

    private static async Task<IEnumerable<UserActor>> GetUserActorsAsync(SqlConnection sqlConnection)
    {
        return await sqlConnection.QueryAsync<UserActor>(
            @"SELECT UserId, ActorId
                   FROM [dbo].[UserActor]").ConfigureAwait(false) ?? (IEnumerable<UserActor>)Array.Empty<object>();
    }

    private static async Task SyncGridAreaLinksAsync(SqlConnection actorRegistrySqlConnection, SqlConnection meteringPointSqlConnection, SqlTransaction transaction)
    {
        var gridAreaLinks = actorRegistrySqlConnection.Query<GridAreaLink>(
            @"SELECT [GridLinkId]
                       ,[GridAreaId]
                   FROM [dbo].[GridAreaLinkInfo]") ?? (IEnumerable<GridAreaLink>)Array.Empty<object>();

        foreach (var gridAreaLink in gridAreaLinks)
        {
            await meteringPointSqlConnection.ExecuteAsync(
                "INSERT INTO [dbo].[GridAreaLinks] ([Id],[GridAreaId]) VALUES (@GridLinkId ,@GridAreaId)",
                new { gridAreaLink.GridLinkId, gridAreaLink.GridAreaId },
                transaction).ConfigureAwait(false);
        }
    }

    private static async Task SyncGridAreasAsync(SqlConnection actorRegistrySqlConnection, SqlConnection meteringPointSqlConnection, SqlTransaction transaction)
    {
        var gridAreas = actorRegistrySqlConnection.Query<GridArea>(
            @"SELECT [Code]
                       ,[Name]
                       ,[Active]
                       ,[ActorId]
                       ,[PriceAreaCode]
                       ,[Id]
                  FROM [dbo].[GridArea]") ?? (IEnumerable<GridArea>)Array.Empty<object>();

        foreach (var gridArea in gridAreas)
        {
            await meteringPointSqlConnection.ExecuteAsync(
                "INSERT INTO [dbo].[GridAreas]([Id],[Code],[Name],[PriceAreaCode],[FullFlexFromDate],[ActorId]) VALUES (@Id, @Code, @Name, @PriceAreaCode, null, @ActorId)",
                new
                {
                    gridArea.Id,
                    gridArea.Code,
                    gridArea.Name,
                    gridArea.PriceAreaCode,
                    gridArea.ActorId,
                },
                transaction).ConfigureAwait(false);
        }
    }

    private static async Task SyncActorsAsync(SqlConnection actorRegistrySqlConnection, SqlConnection meteringPointSqlConnection, SqlTransaction transaction)
    {
        _actors = actorRegistrySqlConnection.Query<Actor>(
            @"SELECT [IdentificationNumber]
                       ,[IdentificationType]
                       ,[Roles]
                       ,[Active]
                       ,[Id]
        FROM [dbo].[Actor]") ?? (IEnumerable<Actor>)Array.Empty<object>();

        foreach (var actor in _actors)
        {
            await meteringPointSqlConnection.ExecuteAsync(
                "INSERT INTO [dbo].[Actor] ([Id],[IdentificationNumber],[IdentificationType],[Roles]) VALUES (@Id,@IdentificationNumber,@IdentificationType, @Roles)",
                new
                {
                    actor.Id, actor.IdentificationNumber, IdentificationType = GetType(actor.IdentificationType), Roles = GetRoles(actor.Roles),
                },
                transaction).ConfigureAwait(false);
        }
    }

    private static string GetType(int identificationType)
    {
        return identificationType == 1 ? "GLN" : "EIC";
    }
}
