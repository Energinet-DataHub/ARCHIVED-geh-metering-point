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
using Dapper;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Production;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Production
{
    public class OnProductionMeteringPointCreated : IntegrationEventPublisher<ProductionMeteringPointCreated>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public OnProductionMeteringPointCreated(IOutbox outbox, IOutboxMessageFactory outboxMessageFactory, IDbConnectionFactory connectionFactory)
            : base(outbox, outboxMessageFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public override async Task Handle(ProductionMeteringPointCreated notification, CancellationToken cancellationToken)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            var gridAreaCode = await GetGridAreaCodeAsync(notification.GridAreaLinkId).ConfigureAwait(false);
            var message = new ProductionMeteringPointCreatedIntegrationEvent(
                notification.MeteringPointId.ToString(),
                notification.GsrnNumber,
                gridAreaCode,
                notification.MeteringPointSubType,
                notification.ReadingOccurrence,
                notification.NetSettlementGroup,
                notification.ProductType,
                notification.PhysicalState,
                notification.UnitType,
                notification.EffectiveDate);

            CreateAndAddOutboxMessage(message);
        }

        private async Task<string> GetGridAreaCodeAsync(Guid gridAreaLinkId)
        {
            var sql = @"SELECT GridAreas.Code FROM GridAreas
                        INNER JOIN GridAreaLinks ON GridAreas.Id = GridAreaLinks.GridAreaId
                        WHERE GridAreaLinks.Id =@GridAreaLinkId";
            var result = await _connectionFactory
                .GetOpenConnection()
                .ExecuteScalarAsync<string?>(sql, new { gridAreaLinkId })
                .ConfigureAwait(false);

            return result ?? throw new InvalidOperationException("Grid Area Code not found");
        }
    }
}
