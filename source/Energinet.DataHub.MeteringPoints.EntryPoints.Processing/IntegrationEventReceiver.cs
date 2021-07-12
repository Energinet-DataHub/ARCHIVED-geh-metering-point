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
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
{
    public class IntegrationEventReceiver
    {
        private readonly ProtobufInboundMapperFactory _protobufInboundMapperFactory;
        private readonly IMediator _mediator;
        private readonly IProtobufMessageFactory _protobufMessageFactory;

        public IntegrationEventReceiver(ProtobufInboundMapperFactory protobufInboundMapperFactory, IMediator mediator, IProtobufMessageFactory protobufMessageFactory)
        {
            _protobufInboundMapperFactory = protobufInboundMapperFactory ?? throw new ArgumentNullException(nameof(protobufInboundMapperFactory));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _protobufMessageFactory = protobufMessageFactory ?? throw new ArgumentNullException(nameof(protobufMessageFactory));
        }

        [Function("IntegrationEventReceiver")]
        public Task RunAsync([ServiceBusTrigger("%INTEGRATION_EVENT_QUEUE%", Connection = "INTEGRATION_EVENT_QUEUE_CONNECTION")] byte[] data, FunctionContext context)
        {
            var logger = context.GetLogger("IntegrationEventReceiver");

            var eventTypeName = GetEventTypeName(context);
            logger.LogInformation($"Received notification event of type {eventTypeName}.");

            var message = _protobufMessageFactory.CreateMessageFrom(data, eventTypeName);
            var mapper = _protobufInboundMapperFactory.GetMapper(message.GetType());
            var integrationEvent = mapper.Convert(message);

            return _mediator.Publish(integrationEvent);
        }

        private string GetEventTypeName(FunctionContext context)
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
