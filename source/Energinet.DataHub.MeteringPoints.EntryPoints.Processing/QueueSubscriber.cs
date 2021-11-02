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
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Messages;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MasterDataDocument = Energinet.DataHub.MeteringPoints.Application.MarketDocuments.MasterDataDocument;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
{
    public class QueueSubscriber
    {
        private readonly ILogger _logger;
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor _messageExtractor;
        private readonly IMediator _mediator;
        private readonly IMessageReceiver _messageReceiver;

        public QueueSubscriber(
            ILogger logger,
            ICorrelationContext correlationContext,
            MessageExtractor messageExtractor,
            IMediator mediator,
            IMessageReceiver messageReceiver)
        {
            _logger = logger;
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;
            _mediator = mediator;
            _messageReceiver = messageReceiver ?? throw new ArgumentNullException(nameof(messageReceiver));
        }

        [Function("QueueSubscriber")]
        public async Task RunAsync(
            [ServiceBusTrigger("%METERINGPOINT_QUEUE_TOPIC_NAME%", Connection = "METERINGPOINT_QUEUE_CONNECTION_STRING")] byte[] data,
            FunctionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var message = await _messageExtractor.ExtractAsync(data).ConfigureAwait(false);

            // TODO: This must be handled otherwise. Maybe a separate queue for market documents
            if (message is IInternalMarketDocument)
            {
                await _messageReceiver.HandleAsync((MasterDataDocument)message).ConfigureAwait(false);
            }
            else
            {
                await _mediator.Send(message).ConfigureAwait(false);
            }

            _logger.LogInformation("Dequeued with correlation id: {correlationId}", _correlationContext.Id);
        }
    }
}
