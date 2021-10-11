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
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Consumption;
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
        public async Task CreateMeteringPoint_WithNoValidationErrors_ShouldBeRetrievableFromRepository()
        {
            var request = CreateRequest();

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            var gsrnNumber = GsrnNumber.Create(request.GsrnNumber);
            var found = await _meteringPointRepository.GetByGsrnNumberAsync(gsrnNumber).ConfigureAwait(false);
            Assert.NotNull(found);
        }

        [Fact]
        public async Task CreateMeteringPoint_WithNoValidationErrors_ShouldGenerateConfirmMessageInOutbox()
        {
            var request = CreateRequest();

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            AssertOutboxMessage<PostOfficeEnvelope>(envelope => envelope.MessageType == nameof(CreateMeteringPointAccepted));
        }

        [Fact]
        public async Task ConsumptionCreateMeteringPoint_WithNoValidationErrors_ShouldGenerateIntegrationEventInOutbox()
        {
            var request = CreateRequest();

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            AssertOutboxMessage<ConsumptionMeteringPointCreatedIntegrationEvent>();
        }

        [Fact]
        public async Task CreateMeteringPoint_WithValidationErrors_ShouldGenerateRejectMessageInOutbox()
        {
            var request = CreateRequest() with
            {
                GsrnNumber = "This is not a valid GSRN number",
                SettlementMethod = "WrongSettlementMethod",
            };

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            AssertOutboxMessage<PostOfficeEnvelope>(envelope => envelope.MessageType == nameof(CreateMeteringPointRejected));
        }

        [Fact]
        public async Task Should_reject_when_grid_area_doesnt_exist()
        {
            var request = CreateRequest() with { MeteringGridArea = "foo" };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("E10");
        }

        [Fact]
        public async Task CreateMeteringPoint_WithAlreadyExistingGsrnNumber_ShouldGenerateRejectMessageInOutbox()
        {
            var request = CreateRequest();
            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);
            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            AssertOutboxMessage<PostOfficeEnvelope>(envelope => envelope.MessageType == nameof(CreateMeteringPointRejected));
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
            var request = CreateRequest()
                with
                {
                    MeterNumber = null,
                    MeteringMethod = MeteringMethod.Physical.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D02");
        }

        [Fact]
        public async Task Should_reject_if_metering_method_is_not_physical_and_meter_identification_is_defined()
        {
            var request = CreateRequest()
                with
                {
                    MeterNumber = SampleData.MeterNumber,
                    MeteringMethod = MeteringMethod.Virtual.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("E86");
        }

        [Fact]
        public async Task Should_reject_when_capacity_is_required_but_not_specified()
        {
            var request = CreateRequest()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.One.Name,
                    ConnectionType = ConnectionType.Installation.Name,
                    PhysicalConnectionCapacity = null,
                    MeteringMethod = MeteringMethod.Calculated.Name,
                    MeterNumber = null,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D56");
        }

        [Fact]
        public async Task Should_reject_if_connection_type_is_not_allowed()
        {
            var request = CreateRequest()
                with
                {
                    ConnectionType = ConnectionType.Installation.Name,
                    NetSettlementGroup = NetSettlementGroup.Zero.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D02");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("invalid_value")]
        public async Task Should_reject_when_metering_method_is_missing_or_is_invalid(string meteringMethod)
        {
            var request = CreateRequest()
                with
                {
                    MeteringMethod = meteringMethod,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D02");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("invalid_value")]
        public async Task Should_reject_when_measurement_unit_is_missing_or_is_invalid(string measurementUnitType)
        {
            var request = CreateRequest()
                with
                {
                    MeasureUnitType = measurementUnitType,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D02");
        }

        private static MasterDataDocument CreateRequest()
        {
            return new(
                BusinessProcessType.CreateMeteringPoint.Name,
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
                SampleData.EffectiveDate,
                SampleData.MeterNumber,
                SampleData.Transaction,
                SampleData.PhysicalState,
                SampleData.NetSettlementGroup,
                SampleData.ConnectionType,
                SampleData.AssetType,
                "123",
                ToGrid: "456",
                ParentRelatedMeteringPoint: null,
                SampleData.ProductType,
                null,
                SampleData.GeoInfoReference,
                SampleData.MeasurementUnitType,
                SampleData.ScheduledMeterReadingDate);
        }
    }
}
