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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Helpers;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Repository
{
    public class IntegrationEventRepository : IIntegrationEventRepository
    {
        private readonly MeteringPointContext _meteringPointContext;
        private readonly IJsonSerializer _jsonSerializer;

        public IntegrationEventRepository(MeteringPointContext meteringPointContext, IJsonSerializer jsonSerializer)
        {
            _meteringPointContext = meteringPointContext;
            _jsonSerializer = jsonSerializer;
        }

        public async Task<OutboxMessage> GetUnProcessedIntegrationEventMessageAsync()
        {
            return await _meteringPointContext.OutboxMessages
                .Where(x => x.ProcessedDate == null)
                .Select(x => new OutboxMessage(x.Type, x.Data, x.Category, x.CreationDate, x.Id))
                .FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task MarkIntegrationEventMessageAsProcessedAsync(Guid id)
        {
            OutboxMessage eventToBeMarkedAsProcessed = await _meteringPointContext.OutboxMessages
                .SingleAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (eventToBeMarkedAsProcessed != null) eventToBeMarkedAsProcessed.ProcessedDate = SystemClock.Instance.GetCurrentInstant();
        }

        public async Task SaveIntegrationEventMessageToOutboxAsync<TMessage>(TMessage message, OutboxMessageCategory category)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var messageType = message.GetType().FullName;
            var data = _jsonSerializer.Serialize(message);

            var outboxMessage = new OutboxMessage(messageType!, data, category, SystemClock.Instance.GetCurrentInstant());
            await _meteringPointContext.OutboxMessages.AddAsync(outboxMessage);
        }
    }
}
