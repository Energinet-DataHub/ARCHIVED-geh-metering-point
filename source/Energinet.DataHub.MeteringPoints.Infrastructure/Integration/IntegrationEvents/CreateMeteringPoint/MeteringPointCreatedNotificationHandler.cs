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
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using MediatR;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint
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
            var message = new MeteringPointCreatedEventMessage(
                notification.GsrnNumber.Value,
                notification.MeteringPointType.Name,
                notification.GridAreaId.Value.ToString(),
                string.Empty,
                notification.MeteringPointSubType.Name,
                notification.PhysicalState.Name,
                notification.ReadingOccurrence.Name,
                string.Empty,
                string.Empty,
                string.Empty,
                notification.ProductType.Name,
                notification.MeasurementUnitType.Name,
                string.Empty,
                SystemClock.Instance.GetCurrentInstant().ToString());  // TODO: Use actual input properties for this and other missing fields

            // TODO: When parent_MarketEvaluationPoint.mRID is implemented update child property
            var outboxMessage = _outboxMessageFactory.CreateFrom(message, OutboxMessageCategory.IntegrationEvent);
            _outbox.Add(outboxMessage);

            return Task.CompletedTask;
        }
    }
}
