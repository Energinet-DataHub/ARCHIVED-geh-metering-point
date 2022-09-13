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

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.ActorSync.Functions;

public static class ActorSubscriber
{
    [FunctionName("ActorSubscriber")]
    public static async Task RunAsync([ServiceBusTrigger("%METERINGPOINT_ACTOR_QUEUE_NAME%", Connection = "METERINGPOINT_ACTOR_QUEUE_LISTEN_CONNECTION_STRING")] byte[] data, ILogger log)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
