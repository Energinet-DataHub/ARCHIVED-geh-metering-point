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
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.ActorRegistrySync.Entities;
using Energinet.DataHub.MeteringPoints.ActorRegistrySync.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.ActorRegistrySync;

public class GrantUserAccess : IDisposable
{
    private readonly AccessService _accessService;

    public GrantUserAccess()
    {
        _accessService = AccessService.Create();
    }

    [FunctionName("GrantUserAccess")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
    {
        if (req == null) throw new ArgumentNullException(nameof(req));
        if (log == null) throw new ArgumentNullException(nameof(log));

        log.LogInformation("C# HTTP trigger function processed a request.");

        using var streamReader = new StreamReader(req.Body);
        var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);
        var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
        var data = JsonSerializer.Deserialize<UserActorDto>(requestBody, serializerOptions);

        if (data == null || data.UserObjectId == null || data.GlnNumbers == null)
        {
            return new BadRequestObjectResult("Data invalid or properties missing.");
        }

        Guid userObjectId;
        try
        {
            userObjectId = new Guid(data.UserObjectId);
        }
        catch (Exception e) when (e is FormatException or OverflowException)
        {
            return new BadRequestObjectResult("User ID is invalid: \n" + e.Message);
        }

        var userCreatedCount = await _accessService.CreateUserActorPermissionAsync(userObjectId, data.GlnNumbers).ConfigureAwait(false);

        return new OkObjectResult("User permissions updated. \n" +
                                  $"Created {userCreatedCount.UserCount} new user(s).\n" +
                                  $"Created {userCreatedCount.PermissionCount} new permission(s)");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _accessService.Dispose();
        }
    }
}
