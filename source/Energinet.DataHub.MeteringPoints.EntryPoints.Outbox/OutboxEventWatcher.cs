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
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.ActorMessages;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.EventDispatchers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Outbox
{
    internal class OutboxEventWatcher
    {
        private readonly ILogger _logger;
        private readonly IntegrationEventDispatchOrchestrator _integrationEventDispatchOrchestrator;

        public OutboxEventWatcher(
            ILogger logger, IntegrationEventDispatchOrchestrator integrationEventDispatchOrchestrator)
        {
            _logger = logger;
            _integrationEventDispatchOrchestrator = integrationEventDispatchOrchestrator;
        }

        [Function("OutboxEventWatcher")]
        public async Task RunAsync(
            [TimerTrigger("%EVENT_MESSAGE_DISPATCH_TRIGGER_TIMER%")] TimerInfo timerInformation)
        {
            _logger.LogInformation($"Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {timerInformation?.ScheduleStatus?.Next}");

            await _integrationEventDispatchOrchestrator.ProcessEventOrchestratorAsync().ConfigureAwait(false);
        }
    }
}
