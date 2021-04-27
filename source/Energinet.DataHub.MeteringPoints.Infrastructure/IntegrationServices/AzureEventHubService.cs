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

using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices
{
    public class AzureEventHubService : IAzureEventHubService, System.IDisposable
    {
        private readonly EventHubProducerClient _client;

        public AzureEventHubService(AzureEventHubConfig azureEventHubConfig)
        {
            if (azureEventHubConfig is not null)
            {
                _client = new EventHubProducerClient(azureEventHubConfig.ConnectionString, azureEventHubConfig.HubName);
            }
        }

        public async Task SendEventAsync<T>(T serviceBusMessage)
        {
            using var eventBatch = await _client.CreateBatchAsync().ConfigureAwait(false);

            // we convert the annonymous obj to a json
            eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(serviceBusMessage))));
            await _client.SendAsync(eventBatch).ConfigureAwait(false);
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
