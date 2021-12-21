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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint
{
    public class MeteringPointCreatedNotificationHandler
        : IntegrationEventPublisher<MeteringPointCreated>
    {
        public MeteringPointCreatedNotificationHandler(IOutbox outbox, IOutboxMessageFactory outboxMessageFactory)
            : base(outbox, outboxMessageFactory)
        {
        }

        public override Task Handle(
            MeteringPointCreated notification,
            CancellationToken cancellationToken)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            var message = new MeteringPointCreatedEventMessage(
                notification.MeteringPointId.ToString(),
                notification.GsrnNumber,
                notification.MeteringPointType,
                notification.GridAreaLinkId.ToString(),
                notification.SettlementMethod,
                notification.MeteringPointSubType,
                notification.PhysicalState,
                notification.ReadingOccurrence,
                notification.NetSettlementGroup,
                notification.TargetGridAreaLinkId == null ? string.Empty : notification.TargetGridAreaLinkId.Value.ToString(),
                notification.SourceGridAreaLinkId == null ? string.Empty : notification.SourceGridAreaLinkId.Value.ToString(),
                notification.ProductType,
                notification.UnitType,
                string.Empty,
                notification.OccurredOn);

            CreateAndAddOutboxMessage(message);

            return Task.CompletedTask;
        }
    }
}
