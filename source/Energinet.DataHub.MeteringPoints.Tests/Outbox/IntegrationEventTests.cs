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
using Energinet.DataHub.MeteringPoints.Application.IntegrationEvent;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Repository;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Outbox
{
    [UnitTest]
    public class IntegrationEventTests
    {
        private readonly Mock<IIntegrationEventDispatchOrchestrator> _integrationEventDispatchOrchestratorMock;
        private readonly Mock<IIntegrationEventRepository> _integrationEventRepositoryMock;

        public IntegrationEventTests()
        {
            _integrationEventRepositoryMock = new Mock<IIntegrationEventRepository>();
            _integrationEventDispatchOrchestratorMock = new Mock<IIntegrationEventDispatchOrchestrator>();
            _integrationEventRepositoryMock.SetupSequence(x => x.GetUnProcessedIntegrationEventMessageAsync())
                .ReturnsAsync(new OutboxMessage(
                    "CreateMeteringPointEventMessage",
                    "{\"Gsrn\":\"000000000\",\"MpType\":\"CreateMeteringPointEventMessage\",\"GridAccessProvider\":\"GridAccessProvider\",\"Child\":true,\"EnergySupplierCurrent\":\"EnergySupplierCurrent\"}",
                    OutboxMessageCategory.IntegrationEvent,
                    SystemClock.Instance.GetCurrentInstant(),
                    Guid.NewGuid()))
                .ReturnsAsync(new OutboxMessage(
                    "CreateMeteringPointEventMessage",
                    "{\"Gsrn\":\"98271293\",\"MpType\":\"CreateMeteringPointEventMessage\",\"GridAccessProvider\":\"GridAccessProvider\",\"Child\":true,\"EnergySupplierCurrent\":\"EnergySupplierCurrent\"}",
                    OutboxMessageCategory.IntegrationEvent,
                    SystemClock.Instance.GetCurrentInstant(),
                    Guid.NewGuid()))
                .ReturnsAsync((OutboxMessage)null);
        }
    }
}
