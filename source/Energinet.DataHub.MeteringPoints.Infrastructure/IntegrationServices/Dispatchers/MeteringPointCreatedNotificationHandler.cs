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
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Repository;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Dispatchers
{
    public class
        MeteringPointCreatedNotificationHandler : INotificationHandler<Domain.MeteringPoints.MeteringPointCreated>
    {
        private readonly IOutbox _outbox;
        private readonly IOutboxMessageFactory _outboxMessageFactory;

        public MeteringPointCreatedNotificationHandler(IOutbox outbox, IOutboxMessageFactory outboxMessageFactory)
        {
            _outbox = outbox;
            _outboxMessageFactory = outboxMessageFactory;
        }

        public Task Handle(
            Domain.MeteringPoints.MeteringPointCreated notification,
            CancellationToken cancellationToken)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            var message = new CreateMeteringPointEventMessage(
                notification.GsrnNumber.Value,
                "notification.mptype",
                "notification.GridAccessProvider",
                true,
                "notification.EnergySupplierCurrent");

            var outboxMessage = _outboxMessageFactory.CreateFrom(message, OutboxMessageCategory.IntegrationEvent);
            _outbox.Add(outboxMessage);

            return Task.CompletedTask;
        }
    }
}
