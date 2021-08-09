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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.Connect
{
    public class OnMeteringPointConnected : INotificationHandler<MeteringPointConnected>
    {
        private readonly IOutbox _outbox;
        private readonly IOutboxMessageFactory _outboxMessageFactory;

        public OnMeteringPointConnected(IOutbox outbox, IOutboxMessageFactory outboxMessageFactory)
        {
            _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
            _outboxMessageFactory = outboxMessageFactory ?? throw new ArgumentNullException(nameof(outboxMessageFactory));
        }

        public Task Handle(MeteringPointConnected notification, CancellationToken cancellationToken)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            var integrationEvent = new MeteringPointConnectedIntegrationEvent(notification.MeteringPointId, notification.GsrnNumber, notification.EffectiveDate.ToDateTimeUtc().ToShortDateString());

            var outboxMessage = _outboxMessageFactory.CreateFrom(integrationEvent, OutboxMessageCategory.IntegrationEvent);
            _outbox.Add(outboxMessage);

            return Task.CompletedTask;
        }
    }
}
