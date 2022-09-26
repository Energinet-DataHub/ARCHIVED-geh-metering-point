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

    private static IEnumerable<UserActor> MapGlnAndUserToUserActor(UserActorDto userActor, IReadOnlyCollection<MeteringPointActor> actorCache)
    {
        return
            from glnNumber in userActor.GlnNumbers
            join actor in actorCache
                on glnNumber equals actor.IdentificationNumber
            select new UserActor(userActor.UserObjectId, actor.Id);
    }

    private static IReadOnlyCollection<string> MapUniqueGlnNumbers(IReadOnlyCollection<UserActorDto> userActorDtos)
    {
        return userActorDtos.SelectMany(userActor => userActor.GlnNumbers).Distinct().ToList().AsReadOnly();
    }

    private static IReadOnlyCollection<Guid> MapUserIds(IReadOnlyCollection<UserActorDto> userActorDtos)
    {
        return userActorDtos.Select(userActor => userActor.UserObjectId).ToList().AsReadOnly();
    }

    private static IEnumerable<UserActor> FilterExistingPermissions(IEnumerable<UserActor> userActors, IEnumerable<UserActor> usersCurrentPermissions)
    {
        return userActors.Where(userActor => !usersCurrentPermissions.Any(existingUserActor => userActor.ActorId == existingUserActor.ActorId && userActor.UserId == existingUserActor.UserId));
    }

    private async Task<IReadOnlyCollection<MeteringPointActor>> GetActorsByGlnNumbersAsync(List<string> glnNumbers)
    {
        return (await _meteringPointDbService.GetActorIdsByGlnNumbersAsync(glnNumbers).ConfigureAwait(false)).ToList().AsReadOnly();
    }
}
