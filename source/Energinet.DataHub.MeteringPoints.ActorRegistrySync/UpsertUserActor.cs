// Copyright 2022 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.ActorRegistrySync;

public static class UpsertUserActor
{
    [Function("UpsertUserActor")]
    public static async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        if (req is null)
        {
            throw new ArgumentNullException(nameof(req));
        }

        dynamic? data;
        using (var reader = new StreamReader(req.Body, Encoding.UTF8))
        {
            data = JsonSerializer.Deserialize<dynamic>(await reader.ReadToEndAsync().ConfigureAwait(false));
        }

        string bodyStr;

        bodyStr = data?.name != null ? $"Hello, {data.name}" : "Please pass a name on the query string or in the request body";

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Date", "Mon, 18 Jul 2016 16:06:00 GMT");
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        await response.WriteStringAsync(bodyStr).ConfigureAwait(false);

        return response;
    }
}
