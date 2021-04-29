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
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Energinet.DataHub.MeteringPoints.Application.Transport;
using Energinet.DataHub.MeteringPoints.Contracts;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices
{
    public sealed class AzureEventHubService : Channel, IAzureEventHubService, System.IDisposable
    {
        private readonly EventHubProducerClient _client;

        public AzureEventHubService(AzureEventHubConfig azureEventHubConfig)
        {
            if (azureEventHubConfig is not null)
            {
                _client = new EventHubProducerClient(azureEventHubConfig.ConnectionString, azureEventHubConfig.HubName);
            }
        }

        public async Task SendEventAsync<T>(T eventMessage)
        {
            using var eventBatch = await _client.CreateBatchAsync().ConfigureAwait(false);
            Type t = eventMessage.GetType();
            var eventContract = new EventContract { Gsrn = "smth" };
            // we convert the annonymous obj to a json
            eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(eventMessage))));
            await _client.SendAsync(eventBatch).ConfigureAwait(false);
        }

        public void Dispose()
        {
           _client?.DisposeAsync();
        }

        protected override Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
