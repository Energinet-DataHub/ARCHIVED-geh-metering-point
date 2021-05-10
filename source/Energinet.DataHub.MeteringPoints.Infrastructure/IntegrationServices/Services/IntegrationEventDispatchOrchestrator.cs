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

using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.IntegrationEvent;
using Energinet.DataHub.MeteringPoints.Application.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Helpers;
using MediatR;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Services
{
    public class IntegrationEventDispatchOrchestrator : IIntegrationEventDispatchOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly IJsonSerializer _jsonSerializer;

        public IntegrationEventDispatchOrchestrator(IMediator mediator, IJsonSerializer jsonSerializer)
        {
            _mediator = mediator;
            _jsonSerializer = jsonSerializer;
        }

        public async Task ProcessEventOrchestratorAsync()
        {
            // Fetch the first message to process
            MeteringPointOutboxMessage outboxMessage = FetchEventFromOutbox();

            // Keep iterating as long as we have a message
            while (outboxMessage != null)
            {
                object parsedCommand = _jsonSerializer.Deserialize(
                    outboxMessage.Data,
                    IntegrationEventTypeFactory.GetType(outboxMessage.Type));
                await _mediator.Send(parsedCommand, CancellationToken.None).ConfigureAwait(false);
                outboxMessage = FetchEventFromOutbox();
            }
        }

        private MeteringPointOutboxMessage FetchEventFromOutbox()
        {
            return new("987654321", "{\"Gsrn\":\"987654321\",\"MpType\":\"CreateMeteringPointEventMessage\",\"GridAccessProvider\":\"GridAccessProvider\",\"Child\":true,\"EnergySupplierCurrent\":\"EnergySupplierCurrent\"}",
                "CreateMeteringPointEventMessage", "IntegrationEvent", SystemClock.Instance.GetCurrentInstant(), SystemClock.Instance.GetCurrentInstant());
        }
    }
}
