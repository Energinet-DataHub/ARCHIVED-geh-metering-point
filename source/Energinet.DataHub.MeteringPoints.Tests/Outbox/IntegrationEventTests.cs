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
using Energinet.DataHub.MeteringPoints.Application.IntegrationEvent;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Helpers;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Dispatchers;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Helpers;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Repository;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Services;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using MediatR;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;
using CreateMeteringPoint = Energinet.DataHub.MeteringPoints.Application.CreateMeteringPoint;

namespace Energinet.DataHub.MeteringPoints.Tests.Outbox
{
    [UnitTest]
    public class IntegrationEventTests
    {
        private readonly Mock<IIntegrationEventDispatchOrchestrator> _integrationEventDispatchOrchestratorMock;
        private readonly Mock<IIntegrationEventRepository> _integrationEventRepositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly OutboxMessage _outboxMessageNoIdCTOR;

        public IntegrationEventTests()
        {
            _integrationEventRepositoryMock = new Mock<IIntegrationEventRepository>();
            _integrationEventDispatchOrchestratorMock = new Mock<IIntegrationEventDispatchOrchestrator>();
            _mediatorMock = new Mock<IMediator>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _outboxMessageNoIdCTOR = new(
                "CreateMeteringPointEventMessage",
                "{\"Gsrn\":\"98271293\",\"MpType\":\"CreateMeteringPointEventMessage\",\"GridAccessProvider\":\"GridAccessProvider\",\"Child\":true,\"EnergySupplierCurrent\":\"EnergySupplierCurrent\"}",
                OutboxMessageCategory.IntegrationEvent,
                SystemClock.Instance.GetCurrentInstant());

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

            _integrationEventRepositoryMock.Setup(m => m.MarkIntegrationEventMessageAsProcessedAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(typeof(void)));
        }

        [Fact]
        public async Task IntegrationEventDispatcherOrchestratorTest()
        {
            var sut = new IntegrationEventDispatchOrchestrator(
                _mediatorMock.Object,
                _jsonSerializerMock.Object,
                _integrationEventRepositoryMock.Object,
                _unitOfWorkMock.Object);
            await sut.ProcessEventOrchestratorAsync().ConfigureAwait(false);

            _integrationEventRepositoryMock.Verify(x => x.GetUnProcessedIntegrationEventMessageAsync(), Times.Exactly(3));
            _integrationEventRepositoryMock.Verify(x => x.MarkIntegrationEventMessageAsProcessedAsync(It.IsAny<Guid>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetUnProcessedIntegrationEventMessageAsyncTest()
        {
            var x = await _integrationEventRepositoryMock.Object.GetUnProcessedIntegrationEventMessageAsync().ConfigureAwait(false);

            Assert.Equal("CreateMeteringPointEventMessage", x.Type);
        }

        [Fact]
        public void IntegrationEventTypeFactorySuccessTest()
        {
            IJsonSerializer jsonSerializer = new JsonSerializer();
            var parsedCommand = jsonSerializer.Deserialize(
                                    _outboxMessageNoIdCTOR.Data,
                                    IntegrationEventTypeFactory.GetType(_outboxMessageNoIdCTOR.Type));

            Assert.IsType<CreateMeteringPointEventMessage>(parsedCommand);
        }

        [Fact]
        public void IntegrationEventTypeFactoryFailTest()
        {
            IJsonSerializer jsonSerializer = new JsonSerializer();
            Assert.Throws<ArgumentException>(() => jsonSerializer.Deserialize(
                _outboxMessageNoIdCTOR.Data,
                IntegrationEventTypeFactory.GetType(typeof(CreateMeteringPoint).ToString())));
        }

        [Fact]
        public void OutBoxMessageGettersSettersTest()
        {
            _outboxMessageNoIdCTOR.ProcessedDate = SystemClock.Instance.GetCurrentInstant();

            Assert.IsType<Guid>(_outboxMessageNoIdCTOR.Id);
            Assert.IsType<Instant>(_outboxMessageNoIdCTOR.CreationDate);
            Assert.IsType<Instant>(_outboxMessageNoIdCTOR.ProcessedDate);
            Assert.Equal(OutboxMessageCategory.IntegrationEvent, _outboxMessageNoIdCTOR.Category);
        }
    }
}
