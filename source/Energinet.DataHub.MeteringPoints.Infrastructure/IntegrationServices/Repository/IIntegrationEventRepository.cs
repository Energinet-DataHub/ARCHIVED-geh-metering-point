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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Repository
{
    /// <summary>
    /// Interface for the event repository
    /// </summary>
    public interface IIntegrationEventRepository
    {
        /// <summary>
        /// Gets unprocessed integration events messages from the outbox sql table
        /// </summary>
        Task<OutboxMessage> GetUnProcessedIntegrationEventMessageAsync();

        /// <summary>
        /// Updates the message in the database to processed
        /// </summary>
        Task MarkIntegrationEventMessageAsProcessedAsync(Guid id);

        /// <summary>
        /// Saves a new integration event message to the outbox
        /// </summary>
        /// <param name="message"></param>
        /// <param name="category"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task SaveIntegrationEventMessageToOutboxAsync<TMessage>(TMessage message, OutboxMessageCategory category);
    }
}
