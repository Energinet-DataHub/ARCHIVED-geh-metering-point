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

using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.Common
{
    internal class OutboxOrchestrator
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOutboxManager _outbox;
        private readonly IOutboxMessageDispatcher _outboxMessageDispatcher;
        private readonly Container _container;

        public OutboxOrchestrator(IUnitOfWork unitOfWork, IOutboxManager outbox, IOutboxMessageDispatcher outboxMessageDispatcher, Container container)
        {
            _unitOfWork = unitOfWork;
            _outbox = outbox;
            _outboxMessageDispatcher = outboxMessageDispatcher;
            _container = container;
        }

        public async Task ProcessOutboxMessagesAsync()
        {
            // Keep iterating as long as we have a message
            while (true)
            {
                await using (Scope scope = AsyncScopedLifestyle.BeginScope(_container))
                {
                    var message = _outbox.GetNext();
                    if (message == null) break;

                    await _outboxMessageDispatcher.DispatchMessageAsync(message).ConfigureAwait(false);

                    _outbox.MarkProcessed(message);

                    await _unitOfWork.CommitAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
