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

using System;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Consumption
{
    public class OnConsumptionMeteringPointCreated : IntegrationEventPublisher<ConsumptionMeteringPointCreated>
    {
        public OnConsumptionMeteringPointCreated(IOutbox outbox, IOutboxMessageFactory outboxMessageFactory)
            : base(outbox, outboxMessageFactory)
        {
        }

        public override Task Handle(ConsumptionMeteringPointCreated notification, CancellationToken cancellationToken)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            var message = new ConsumptionMeteringPointCreatedIntegrationEvent(
                notification.MeteringPointId.ToString(),
                notification.GsrnNumber,
                notification.GridAreaId.ToString(),
                notification.SettlementMethod,
                notification.MeteringPointSubType,
                notification.ReadingOccurrence,
                notification.NetSettlementGroup,
                notification.ProductType,
                notification.EffectiveDate.ToString());

            CreateAndAddOutboxMessage(message);

            return Task.CompletedTask;
        }
    }
}
