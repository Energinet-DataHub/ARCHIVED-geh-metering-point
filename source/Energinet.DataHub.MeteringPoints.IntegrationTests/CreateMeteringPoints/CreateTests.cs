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
using Energinet.DataHub.MeteringPoints.Application.Create.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Consumption;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Production;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CreateMeteringPoints
{
    [IntegrationTest]
    public class CreateTests
        : TestHost
    {
        private readonly IMeteringPointRepository _meteringPointRepository;

        public CreateTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _meteringPointRepository = GetService<IMeteringPointRepository>();
        }

        [Fact]
        public async Task Should_reject_when_grid_area_doesnt_exist()
        {
            var request = CreateCommand() with { MeteringGridArea = "foo" };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E10", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task CreateMeteringPoint_WithAlreadyExistingGsrnNumber_ShouldGenerateRejectMessageInOutbox()
        {
            var request = CreateCommand();
            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);
            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            AssertOutboxMessage<MessageHubEnvelope>(envelope => envelope.MessageType == DocumentType.CreateMeteringPointRejected);
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

        [Fact]
        public async Task Should_reject_if_metering_method_is_physical_and_meter_identification_is_undefined()
        {
            var request = CreateCommand()
                with
                {
                    ConnectionType = string.Empty,
                    ScheduledMeterReadingDate = string.Empty,
                    NetSettlementGroup = NetSettlementGroup.Zero.Name,
                    MeterNumber = null,
                    MeteringMethod = MeteringMethod.Physical.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D31", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_if_metering_method_is_not_physical_and_meter_identification_is_defined()
        {
            var request = CreateCommand()
                with
                {
                    MeterNumber = SampleData.MeterNumber,
                    MeteringMethod = MeteringMethod.Virtual.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_when_capacity_is_required_but_not_specified()
        {
            var request = CreateCommand()
                with
                {
                    ConnectionType = ConnectionType.Installation.Name,
                    PhysicalConnectionCapacity = null,
                    MeteringMethod = MeteringMethod.Calculated.Name,
                    MeterNumber = null,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D56", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_if_connection_type_is_not_allowed()
        {
            var request = CreateCommand()
                with
                {
                    ConnectionType = ConnectionType.Installation.Name,
                    NetSettlementGroup = NetSettlementGroup.Zero.Name,
                    ScheduledMeterReadingDate = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_when_metering_method_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    MeteringMethod = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.CreateMeteringPointRejected);
        }

        private static CreateConsumptionMeteringPoint CreateCommand()
        {
            return Scenarios.CreateConsumptionMeteringPointCommand();
        }
    }
}
