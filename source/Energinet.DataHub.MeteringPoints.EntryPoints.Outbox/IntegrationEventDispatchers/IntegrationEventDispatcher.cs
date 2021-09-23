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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MeteringPoints.Application.Common.Transport;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Google.Protobuf;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.IntegrationEventDispatchers
{
    public abstract class IntegrationEventDispatcher<TTopic, TEvent> : IRequestHandler<TEvent>
        where TTopic : Topic
        where TEvent : IOutboundMessage, IRequest<Unit>
    {
        private readonly ProtobufOutboundMapper<TEvent> _mapper;
        private readonly IIntegrationMetaDataContext _integrationMetaDataContext;

        protected IntegrationEventDispatcher(ProtobufOutboundMapper<TEvent> mapper, IIntegrationMetaDataContext integrationMetaDataContext)
        {
            _mapper = mapper;
            _integrationMetaDataContext = integrationMetaDataContext;
        }

        public async Task<Unit> Handle(TEvent request, CancellationToken cancellationToken)
        {
            var message = _mapper.Convert(request);
            var bytes = message.ToByteArray();
            ServiceBusMessage serviceBusMessage = new(bytes)
            {
                ContentType = "application/octet-stream;charset=utf-8",
            };
            serviceBusMessage.ApplicationProperties.Add("Timestamp", _integrationMetaDataContext.Timestamp.ToString());
            serviceBusMessage.ApplicationProperties.Add("CorrelationId", _integrationMetaDataContext.CorrelationId);
            serviceBusMessage.ApplicationProperties.Add("EventIdentifier", _integrationMetaDataContext.EventId.ToString());

            await DispatchMessageAsync(serviceBusMessage).ConfigureAwait(false);
            return Unit.Value;
        }

        protected abstract Task DispatchMessageAsync(ServiceBusMessage serviceBusMessage);
    }
}
