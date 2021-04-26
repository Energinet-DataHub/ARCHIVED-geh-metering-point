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
using Microsoft.Azure.ServiceBus;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices
{
    public class AzureBusService : IAzureBusService
    {
        private QueueClient _client;

        public AzureBusService(AzureServiceBusConfig azureServiceBusConfig)
        {
            if (azureServiceBusConfig is not null)
            {
                _client = new QueueClient(azureServiceBusConfig.ConnectionString, azureServiceBusConfig.QueueName);
            }
        }

        public async Task SendMessageAsync(object serviceBusMessage)
        {
            // we convert the annonymous obj to a json
            var msg = new Message(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(serviceBusMessage)));
            await _client.SendAsync(msg).ConfigureAwait(false);
        }
    }
}
