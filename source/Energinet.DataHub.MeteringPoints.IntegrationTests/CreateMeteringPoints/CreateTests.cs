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
using Energinet.DataHub.MeteringPoints.Application.Create;
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
        public async Task CreateConsumptionMeteringPoint_WithNoValidationErrors_ShouldBeRetrievableFromRepository()
        {
            var request = CreateRequest();

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            var gsrnNumber = GsrnNumber.Create(request.GsrnNumber);
            var found = await _meteringPointRepository.GetByGsrnNumberAsync(gsrnNumber).ConfigureAwait(false);
            Assert.NotNull(found);
        }

        [Fact]
        public async Task CreateProductionMeteringPoint_WithNoValidationErrors_ShouldBeRetrievableFromRepository()
        {
            var request = CreateRequest() with
                {
                    TypeOfMeteringPoint = nameof(MeteringPointType.Production),
                };

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            var gsrnNumber = GsrnNumber.Create(request.GsrnNumber);
            var found = await _meteringPointRepository.GetByGsrnNumberAsync(gsrnNumber).ConfigureAwait(false);
            Assert.NotNull(found);
        }

        [Fact]
        public async Task CreateConsumptionMeteringPoint_WithNoValidationErrors_ShouldGenerateConfirmMessageInOutbox()
        {
            var request = CreateRequest();

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            AssertOutboxMessage<MessageHubEnvelope>(envelope => envelope.MessageType == DocumentType.CreateMeteringPointAccepted);
        }

        [Fact]
        public async Task CreateProductionMeteringPoint_WithNoValidationErrors_ShouldGenerateConfirmMessageInOutbox()
        {
            var request = CreateRequest() with
            {
                TypeOfMeteringPoint = nameof(MeteringPointType.Production),
            };

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            AssertOutboxMessage<MessageHubEnvelope>(envelope => envelope.MessageType == DocumentType.CreateMeteringPointAccepted);
        }

        [Fact]
        public async Task ConsumptionCreateMeteringPoint_WithNoValidationErrors_ShouldGenerateIntegrationEventInOutbox()
        {
            var request = CreateRequest();

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            AssertOutboxMessage<ConsumptionMeteringPointCreatedIntegrationEvent>();
        }

        [Fact]
        public async Task ProductionCreateMeteringPoint_WithNoValidationErrors_ShouldGenerateIntegrationEventInOutbox()
        {
            var request = CreateRequest() with
            {
                TypeOfMeteringPoint = nameof(MeteringPointType.Production),
            };

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            AssertOutboxMessage<ProductionMeteringPointCreatedIntegrationEvent>();
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

            AssertOutboxMessage<MessageHubEnvelope>(envelope => envelope.MessageType == DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_when_grid_area_doesnt_exist()
        {
            var request = CreateRequest() with { MeteringGridArea = "foo" };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E10", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task CreateMeteringPoint_WithAlreadyExistingGsrnNumber_ShouldGenerateRejectMessageInOutbox()
        {
            var request = CreateRequest();
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
        public async Task Should_reject_when_maximum_power_is_invalid()
        {
            var invalidPowerLimit = 12345567;
            var request = CreateRequest()
                with
                {
                    MaximumPower = invalidPowerLimit,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_when_maximum_current_is_invalid()
        {
            var invalidCurrent = 12345567;
            var request = CreateRequest()
                with
                {
                    MaximumCurrent = invalidCurrent,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_when_location_description_is_invalid()
        {
            var invalidLocationDescription = "1234567890123456789012345678901234567890123456789012345678901234567890";
            var request = CreateRequest()
                with
                {
                    LocationDescription = invalidLocationDescription,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.CreateMeteringPointRejected);
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

            AssertValidationError("D02", DocumentType.CreateMeteringPointRejected);
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

            AssertValidationError("E86", DocumentType.CreateMeteringPointRejected);
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

            AssertValidationError("D56", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_if_capacity_is_invalid()
        {
            var request = CreateRequest()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.One.Name,
                    PhysicalConnectionCapacity = "123.3333670",
                    MeteringMethod = MeteringMethod.Calculated.Name,
                    MeterNumber = null,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_when_geo_info_reference_is_invalid()
        {
            var invalidGeoInfoReference = "xxxxxxx";
            var request = CreateRequest()
                with
                {
                    GeoInfoReference = invalidGeoInfoReference,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_when_geo_info_reference_is_specified_and_official_address_is_empty()
        {
            var request = CreateRequest()
                with
                {
                    GeoInfoReference = SampleData.GeoInfoReference,
                    IsOfficialAddress = null,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D63", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_if_connection_type_is_unknown()
        {
            var invalidConnectionType = "invalid_value";
            var request = CreateRequest()
                with
                {
                    ConnectionType = invalidConnectionType,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.CreateMeteringPointRejected);
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

            AssertValidationError("D02", DocumentType.CreateMeteringPointRejected);
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

            AssertValidationError("D02", DocumentType.CreateMeteringPointRejected);
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

            AssertValidationError("D02", DocumentType.CreateMeteringPointRejected);
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
                "1000",
                SampleData.GeoInfoReference,
                SampleData.MeasurementUnitType,
                SampleData.ScheduledMeterReadingDate);
        }
    }
}
