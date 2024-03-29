﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.MeteringPoints.Application.Integrations;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Google.Protobuf;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.IntegrationEventDispatchers
{
    public abstract class IntegrationEventDispatcher<TTopic, TEvent> : IRequestHandler<TEvent>
        where TTopic : Topic
        where TEvent : IOutboundMessage, IRequest<Unit>
    {
        private readonly ITopicSender<TTopic> _topicSender;
        private readonly ProtobufOutboundMapperFactory _factory;
        private readonly IIntegrationEventMessageFactory _integrationEventMessageFactory;
        private readonly IIntegrationMetadataContext _integrationMetadataContext;

        protected IntegrationEventDispatcher(
            ITopicSender<TTopic> topicSender,
            ProtobufOutboundMapperFactory factory,
            IIntegrationEventMessageFactory integrationEventMessageFactory,
            IIntegrationMetadataContext integrationMetadataContext)
        {
            _topicSender = topicSender;
            _factory = factory;
            _integrationEventMessageFactory = integrationEventMessageFactory;
            _integrationMetadataContext = integrationMetadataContext;
        }

        public async Task<Unit> Handle(TEvent request, CancellationToken cancellationToken)
        {
            var mapper = _factory.GetMapper(request);
            var message = mapper.Convert(request);
            var bytes = message.ToByteArray();

            var serviceBusMessage = _integrationEventMessageFactory.CreateMessage(bytes, _integrationMetadataContext);
            serviceBusMessage.SetMetadata(_integrationMetadataContext.Timestamp, _integrationMetadataContext.CorrelationId ?? string.Empty, _integrationMetadataContext.EventId);
            EnrichMessage(serviceBusMessage);

            await _topicSender.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);
            await SendExtraMessageIfNeededAsync(serviceBusMessage).ConfigureAwait(false);

            return Unit.Value;
        }

        protected abstract void EnrichMessage(ServiceBusMessage serviceBusMessage);

        protected abstract Task SendExtraMessageIfNeededAsync(ServiceBusMessage serviceBusMessage);
    }
}
