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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Actor;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.ServiceBus;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion
{
    public class InternalServiceBus : Channel
    {
        private readonly IActorContext _actorContext;
        private readonly ICorrelationContext _correlationContext;
        private readonly ServiceBusSender _sender;

        public InternalServiceBus(
            IActorContext actorContext,
            ICorrelationContext correlationContext,
            ServiceBusSender sender)
        {
            _actorContext = actorContext;
            _correlationContext = correlationContext;
            _sender = sender;
        }

        public override async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            if (_actorContext.CurrentActor is null)
                throw new InvalidOperationException("Can't send message when current actor is not set (null)");

            var message = new ServiceBusMessage(data);
            message.CorrelationId = _correlationContext.Id;
            message.ApplicationProperties.Add(Constants.ServiceBusIdentityKey, _actorContext.CurrentActor.AsString());

            await _sender.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
        }
    }
}
