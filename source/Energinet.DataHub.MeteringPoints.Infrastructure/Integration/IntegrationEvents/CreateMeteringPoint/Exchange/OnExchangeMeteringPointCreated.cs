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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using MeteringPointCreated = Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events.MeteringPointCreated;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Exchange
{
    public class OnExchangeMeteringPointCreated : IntegrationEventPublisher<MeteringPointCreated>
    {
        private readonly DbGridAreaHelper _dbGridAreaHelper;

        public OnExchangeMeteringPointCreated(IOutbox outbox, IOutboxMessageFactory outboxMessageFactory, DbGridAreaHelper dbGridAreaHelper)
            : base(outbox, outboxMessageFactory)
        {
            _dbGridAreaHelper = dbGridAreaHelper;
        }

        public override async Task Handle(MeteringPointCreated notification, CancellationToken cancellationToken)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            if (EnumerationType.FromName<MeteringPointType>(notification.MeteringPointType) ==
                MeteringPointType.Exchange)
            {
                // TODO: Should we avoid making 3 db calls in a row? Find a more suitable way to make read queries
                var gridAreaCode = await _dbGridAreaHelper.GetGridAreaCodeAsync(notification.GridAreaLinkId).ConfigureAwait(false);
                var fromGridAreaCode = await _dbGridAreaHelper.GetGridAreaCodeAsync(notification.SourceGridAreaLinkId.GetValueOrDefault()).ConfigureAwait(false);
                var toGridAreaCode = await _dbGridAreaHelper.GetGridAreaCodeAsync(notification.TargetGridAreaLinkId.GetValueOrDefault()).ConfigureAwait(false);
                var message = new ExchangeMeteringPointCreatedIntegrationEvent(
                    notification.MeteringPointId.ToString(),
                    notification.GsrnNumber,
                    gridAreaCode,
                    notification.MeteringPointSubType,
                    notification.ReadingOccurrence,
                    notification.ProductType,
                    notification.EffectiveDate,
                    notification.PhysicalState,
                    fromGridAreaCode,
                    toGridAreaCode);

                CreateAndAddOutboxMessage(message);
            }
        }
    }
}
