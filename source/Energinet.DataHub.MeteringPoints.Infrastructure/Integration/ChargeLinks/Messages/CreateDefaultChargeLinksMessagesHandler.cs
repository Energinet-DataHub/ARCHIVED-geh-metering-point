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
using Energinet.DataHub.MeteringPoints.Application.Integrations.ChargeLinks.Messages;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.ChargeLinks.Messages
{
    public class CreateDefaultChargeLinksMessagesHandler : IRequestHandler<CreateDefaultChargeLinksMessages>
    {
        private readonly IOutbox _outbox;
        private readonly IOutboxMessageFactory _outboxMessageFactory;

        public CreateDefaultChargeLinksMessagesHandler(IOutbox outbox, IOutboxMessageFactory outboxMessageFactory)
        {
            _outbox = outbox;
            _outboxMessageFactory = outboxMessageFactory;
        }

        public Task<Unit> Handle(CreateDefaultChargeLinksMessages request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var requestDefaultChargeLinks = new RequestDefaultChargeLinksMessages(request.GsrnNumber, request.CorrelationId);
            var message = _outboxMessageFactory.CreateFrom(requestDefaultChargeLinks, OutboxMessageCategory.IntegrationEvent);
            _outbox.Add(message);
            return Task.FromResult(Unit.Value);
        }
    }
}
