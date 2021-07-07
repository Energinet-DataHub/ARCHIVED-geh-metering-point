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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Infrastructure.Messaging.Idempotency;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Google.Protobuf;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
{
    public class IntegrationEventReceiver
    {
        private readonly ProtobufInboundMapperFactory _protobufInboundMapperFactory;
        private readonly IMediator _mediator;

        public IntegrationEventReceiver(ProtobufInboundMapperFactory protobufInboundMapperFactory, IMediator mediator)
        {
            _protobufInboundMapperFactory = protobufInboundMapperFactory ?? throw new ArgumentNullException(nameof(protobufInboundMapperFactory));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("IntegrationEventReceiver")]
        public Task RunAsync([ServiceBusTrigger("%INTEGRATION_EVENT_QUEUE%", Connection = "INTEGRATION_EVENT_QUEUE_CONNECTION")] byte[] data, FunctionContext context)
        {
            var logger = context.GetLogger("IntegrationEventReceiver");

            var eventTypeName = GetEventTypeNameOrThrow(context);
            logger.LogInformation($"Received notification event of type {eventTypeName}.");
            var eventType = GetEventTypeOrThrow(eventTypeName);
            var protobufMessage = ExtractMessageOrThrow(data, eventType);
            var mapper = _protobufInboundMapperFactory.GetMapper(eventType);
            var @event = mapper.Convert(protobufMessage);

            return _mediator.Publish(@event);
        }

        private IMessage ExtractMessageOrThrow(byte[] data, Type eventType)
        {
            if (eventType == null) throw new ArgumentNullException(nameof(eventType));
            var message = Activator.CreateInstance(eventType) as IMessage;
            if (message is null)
            {
                throw new UnknownNotificationEventTypeException(eventType.FullName!);
            }

            return message.Descriptor.Parser.ParseFrom(data);
        }

        private Type GetEventTypeOrThrow(string eventTypeName)
        {
            var eventType = typeof(IIncomingMessageRegistry).Assembly.GetType(eventTypeName);
            if (eventType is null)
            {
                throw new UnknownNotificationEventTypeException(eventTypeName);
            }

            return eventType;
        }

        private string GetEventTypeNameOrThrow(FunctionContext context)
        {
            context.BindingContext.BindingData.TryGetValue("Label", out var label);

            if (label is null)
            {
                throw new InvalidOperationException($"Service bus message must specify the event type using the 'Label/Subject' attribute.");
            }

            return label?.ToString()!;
        }
    }
}
