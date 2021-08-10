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

using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.Outbox
{
    public class OutboxTests
    {
        // TODO: Create Outbox Test
        // [Fact(Skip = "Should not be run with processing configuration. Redo with outbox configuration.")]
        // public async Task CreateMeteringPoint_ProcessIntegrationEvent_ShouldMarkAsProcessedIntegrationEventInOutbox()
        // {
        //     var request = CreateRequest();
        //
        //     await _mediator.Send(request, CancellationToken.None).ConfigureAwait(false);
        //     await _integrationEventDispatchOrchestrator.ProcessEventOrchestratorAsync().ConfigureAwait(false);
        //
        //     var outboxMessage = _outbox.GetNext(OutboxMessageCategory.IntegrationEvent);
        //     outboxMessage.Should().BeNull();
        // }
    }
}
