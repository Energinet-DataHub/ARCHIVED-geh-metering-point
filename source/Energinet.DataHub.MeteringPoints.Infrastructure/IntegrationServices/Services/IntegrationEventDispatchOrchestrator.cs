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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.IntegrationEvent;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Helpers;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Helpers;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Repository;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Services
{
    public class IntegrationEventDispatchOrchestrator : IIntegrationEventDispatchOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOutboxManager _outbox;

        public IntegrationEventDispatchOrchestrator(IMediator mediator, IJsonSerializer jsonSerializer, IUnitOfWork unitOfWork, IOutboxManager outbox)
        {
            _mediator = mediator;
            _jsonSerializer = jsonSerializer;
            _unitOfWork = unitOfWork;
            _outbox = outbox;
        }

        public async Task ProcessEventOrchestratorAsync()
        {
            // Fetch the first message to process
            var outboxMessage = FetchEventFromOutbox();

            // Keep iterating as long as we have a message
            while (outboxMessage != null)
            {
                object parsedCommand = _jsonSerializer.Deserialize(
                        outboxMessage.Data,
                        IntegrationEventTypeFactory.GetType(outboxMessage.Type));

                await _mediator.Send(parsedCommand, CancellationToken.None).ConfigureAwait(false);
                _outbox.MarkProcessed(outboxMessage);

                await _unitOfWork.CommitAsync().ConfigureAwait(false);
                outboxMessage = FetchEventFromOutbox();
            }
        }

        private OutboxMessage? FetchEventFromOutbox()
        {
            return _outbox.GetNext(OutboxMessageCategory.IntegrationEvent);
        }
    }
}
