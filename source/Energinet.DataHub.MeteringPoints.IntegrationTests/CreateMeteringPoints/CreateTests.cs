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
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using FluentAssertions;
using MediatR;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CreateMeteringPoints
{
    [IntegrationTest]
    public class CreateTests
        : TestHost
    {
        private readonly IMediator _mediator;
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IOutboxManager _outbox;
        private readonly IIntegrationEventDispatchOrchestrator _integrationEventDispatchOrchestrator;

        public CreateTests()
        {
            _mediator = GetService<IMediator>();
            _meteringPointRepository = GetService<IMeteringPointRepository>();
            _outbox = GetService<IOutboxManager>();
            _integrationEventDispatchOrchestrator = GetService<IIntegrationEventDispatchOrchestrator>();
        }

        [Fact]
        public async Task CreateMeteringPoint_WithNoValidationErrors_ShouldBeRetrievableFromRepository()
        {
            var request = CreateRequest();

            await _mediator.Send(request, CancellationToken.None).ConfigureAwait(false);

            var gsrnNumber = GsrnNumber.Create(request.GsrnNumber);
            var found = await _meteringPointRepository.GetByGsrnNumberAsync(gsrnNumber).ConfigureAwait(false);
            Assert.NotNull(found);
        }

        [Fact]
        public async Task CreateMeteringPoint_WithNoValidationErrors_ShouldGenerateConfirmMessageInOutbox()
        {
            var request = CreateRequest();

            await _mediator.Send(request, CancellationToken.None).ConfigureAwait(false);

            var outboxMessage = _outbox.GetNext(OutboxMessageCategory.ActorMessage);
            outboxMessage.Should().NotBeNull();
            outboxMessage?.Type.Should().Be(typeof(CreateMeteringPointAccepted).FullName);
        }

        [Fact]
        public async Task CreateMeteringPoint_WithNoValidationErrors_ShouldGenerateIntegrationEventInOutbox()
        {
            var request = CreateRequest();

            await _mediator.Send(request, CancellationToken.None).ConfigureAwait(false);

            var outboxMessage = _outbox.GetNext(OutboxMessageCategory.IntegrationEvent);
            outboxMessage.Should().NotBeNull();
            outboxMessage?.Type.Should()
                .Be(typeof(MeteringPointCreatedEventMessage).FullName);
        }

        [Fact(Skip = "Should not be run with processing configuration. Redo with outbox configuration.")]
        public async Task CreateMeteringPoint_ProcessIntegrationEvent_ShouldMarkAsProcessedIntegrationEventInOutbox()
        {
            var request = CreateRequest();

            await _mediator.Send(request, CancellationToken.None).ConfigureAwait(false);
            await _integrationEventDispatchOrchestrator.ProcessEventOrchestratorAsync().ConfigureAwait(false);

            var outboxMessage = _outbox.GetNext(OutboxMessageCategory.IntegrationEvent);
            outboxMessage.Should().BeNull();
        }

        [Fact]
        public async Task CreateMeteringPoint_WithValidationErrors_ShouldGenerateRejectMessageInOutbox()
        {
            var request = CreateRequest() with
            {
                GsrnNumber = "This is not a valid GSRN number",
                SettlementMethod = "WrongSettlementMethod",
            };

            await _mediator.Send(request, CancellationToken.None).ConfigureAwait(false);

            var outboxMessage = _outbox.GetNext(OutboxMessageCategory.ActorMessage);
            outboxMessage.Should().NotBeNull();
            outboxMessage?.Type.Should().Be(typeof(CreateMeteringPointRejected).FullName);
        }

        [Fact(Skip = "Not implemented yet")]
        public void CreateMeteringPoint_WithAlreadyExistingGsrnNumber_ShouldGenerateRejectMessageInOutbox()
        {
        }

        [Fact(Skip = "Not implemented yet")]
        public void CreateMeteringPoint_WithUnknownActor_ShouldGenerateRejectMessageInOutbox()
        {
        }

        [Fact(Skip = "Not implemented yet")]
        public void CreateMeteringPoint_WithUnknownGridArea_ShouldGenerateRejectMessageInOutbox()
        {
        }

        [Fact(Skip = "Not implemented yet")]
        public void CreateMeteringPoint_WithGridAreaNotBelongingToGridOperator_ShouldGenerateRejectMessageInOutbox()
        {
        }

        [Fact(Skip = "Not implemented yet")]
        public void CreateMeteringPoint_WhenEffectiveDateIsOutOfScope_ShouldGenerateRejectMessageInOutbox()
        {
        }

        private static CreateMeteringPoint CreateRequest()
        {
            return new(
                SampleData.StreetName,
                SampleData.BuildingNumber,
                SampleData.PostCode,
                SampleData.CityName,
                SampleData.CitySubDivisionName,
                SampleData.MunicipalityCode,
                SampleData.CountryCode,
                SampleData.StreetCode,
                SampleData.FloorIdentification,
                SampleData.RoomIdentification,
                SampleData.IsWashable,
                SampleData.GsrnNumber,
                SampleData.TypeOfMeteringPoint,
                SampleData.SubTypeOfMeteringPoint,
                SampleData.ReadingOccurrence,
                0,
                0,
                SampleData.MeteringGridArea,
                SampleData.PowerPlantGsrnNumber,
                string.Empty,
                SampleData.SettlementMethod,
                SampleData.MeasurementUnitType,
                SampleData.DisconnectionType,
                SampleData.Occurrence,
                SampleData.MeterNumber,
                string.Empty,
                string.Empty,
                SampleData.NetSettlementGroup,
                SampleData.ConnectionType,
                SampleData.AssetType,
                "123",
                ToGrid: "456",
                ParentRelatedMeteringPoint: string.Empty,
                SampleData.ProductType,
                null,
                SampleData.GeoInfoReference,
                SampleData.MeasurementUnitType);
        }
    }
}
