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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.ActorRegistrySync.Entities;

namespace Energinet.DataHub.MeteringPoints.ActorRegistrySync.Services;

public class AccessService : IDisposable
{
    private readonly MeteringPointDbService _meteringPointDbService;
    private bool _disposed;

    public AccessService(MeteringPointDbService meteringPointDbService)
    {
        _meteringPointDbService = meteringPointDbService;
    }

    public static AccessService Create()
    {
        var connectionString = Environment.GetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING") ??
                               throw new ArgumentNullException("Environment.GetEnvironmentVariable(\"METERINGPOINT_DB_CONNECTION_STRING\")");

        var dbConnection = new MeteringPointDbService(connectionString);

        return new AccessService(dbConnection);
    }

    // public async Task<CreateCountResponse> CreateUserActorPermissionAsync(Guid userObjectId, IReadOnlyCollection<string> glnNumbers)
    // {
    //     var existingActorIdsForUser = await _meteringPointDbService.GetExistingActorIdsAsync(userObjectId, actorIds).ConfigureAwait(false);
    //     var remainingActorIds = actorIds.Where(id => !existingActorIdsForUser.Contains(id)).ToList();
    //
    //     var userCreatedCount = await _meteringPointDbService.CreateUserAsync(userObjectId).ConfigureAwait(false);
    //     var permissionsCreatedCount = await _meteringPointDbService.CreateUserActorPermissionsAsync(userObjectId, remainingActorIds).ConfigureAwait(false);
    //     return new CreateCountResponse(userCreatedCount, permissionsCreatedCount);
    // }
    public async Task<CreateCountResponse> GrantUserActorPermissionAsync(IReadOnlyCollection<UserActorDto> userActorDtos)
    {
        if (userActorDtos == null) throw new ArgumentNullException(nameof(userActorDtos));

        var actorCache = await GetActorsByGlnNumbersAsync(MapUniqueGlnNumbers(userActorDtos).ToList()).ConfigureAwait(false);

        List<Guid> allUserIds = new();
        List<UserActor> allUserActors = new();
        foreach (var userActor in userActorDtos)
        {
            var userActors =
                from glnNumber in userActor.GlnNumbers
                join actor in actorCache
                    on glnNumber equals actor.IdentificationNumber
                select new UserActor(userActor.UserObjectId, actor.Id);

            allUserIds.Add(userActor.UserObjectId);
            allUserActors.AddRange(userActors);
        }

        await _meteringPointDbService.CreateUsersAsync(allUserIds).ConfigureAwait(false);
        await _meteringPointDbService.InsertUserActorsAsync(allUserActors).ConfigureAwait(false);

        return new CreateCountResponse(999, 999);
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
            _meteringPointDbService.Dispose();
        }

        _disposed = true;
    }

    private static IReadOnlyCollection<string> MapUniqueGlnNumbers(IReadOnlyCollection<UserActorDto> userActorDtos)
    {
        return userActorDtos.SelectMany(userActor => userActor.GlnNumbers).Distinct().ToList().AsReadOnly();
    }

    private async Task<IReadOnlyCollection<Actor>> GetActorsByGlnNumbersAsync(List<string> glnNumbers)
    {
        return (await _meteringPointDbService.GetActorIdsByGlnNumbersAsync(glnNumbers).ConfigureAwait(false)).ToList().AsReadOnly();
    }
}
