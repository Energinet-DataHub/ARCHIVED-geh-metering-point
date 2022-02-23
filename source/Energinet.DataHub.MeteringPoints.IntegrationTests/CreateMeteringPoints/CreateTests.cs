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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.EntryPoints.Processing;
using Energinet.DataHub.MeteringPoints.EntryPoints.WebApi.MeteringPoints.Queries;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using SimpleInjector;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CreateMeteringPoints
{
    [IntegrationTest]
    public class CreateTests
        : EntryPointBasedFixture<Program>
    {
        public CreateTests(DatabaseFixture databaseFixture)
            : base(databaseFixture) { }

        [Fact]
        public async Task Metering_point_created_shows_in_process_overview()
        {
            var request = CreateCommand();

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            await Assert.ProcessOverviewAsync(
                    SampleData.GsrnNumber,
                    "BRS-004",
                    "RequestCreateMeteringPoint",
                    "ConfirmCreateMeteringPoint")
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Reject_if_metering_method_is_not_applicable_to_metering_point_type()
        {
            var request = CreateCommand()
                with
                {
                    MeteringPointType = MeteringPointType.NetConsumption.Name,
                    MeteringMethod = MeteringMethod.Virtual.Name,
                    MeterNumber = string.Empty,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D37");
        }

        [Fact]
        public async Task Should_reject_when_grid_area_doesnt_exist()
        {
            var request = CreateCommand() with { MeteringGridArea = "foo" };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("E10", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task CreateMeteringPoint_WithAlreadyExistingGsrnNumber_ShouldGenerateRejectMessageInOutbox()
        {
            var request = CreateCommand();

            await Act.SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);
            await Act.SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            Assert.OutboxMessage<MessageHubEnvelope>(envelope => envelope.MessageType == DocumentType.RejectCreateMeteringPoint);
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

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D31", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_if_scheduled_meter_reading_date_format_is_wrong()
        {
            var request = CreateCommand()
                with
                {
                    ScheduledMeterReadingDate = "01",
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("E86", DocumentType.RejectCreateMeteringPoint);
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

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("E86", DocumentType.RejectCreateMeteringPoint);
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

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D56", DocumentType.RejectCreateMeteringPoint);
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

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D02", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_metering_method_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    MeteringMethod = string.Empty,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D02", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_metering_method_is_invalid()
        {
            var request = Scenarios.CreateDocument()
                with
                {
                    MeteringMethod = "Invalid_value",
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D02");
        }

        [Fact]
        public async Task Should_reject_if_connection_type_is_unknown()
        {
            var invalidConnectionType = "invalid_value";
            var request = Scenarios.CreateDocument()
                with
                {
                    ConnectionType = invalidConnectionType,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D02");
        }

        [Fact]
        public async Task Should_reject_when_disconnection_type_is_invalid()
        {
            var request = Scenarios.CreateDocument()
                with
                {
                    TypeOfMeteringPoint = nameof(MeteringPointType.Production),
                    DisconnectionType = "invalid_dc_type",
                    PhysicalConnectionCapacity = "1",
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D65");
        }

        [Fact]
        public async Task Should_reject_when_settlement_method_is_invalid()
        {
            var request = Scenarios.CreateDocument()
                with
                {
                    SettlementMethod = "Invalid_Method_Name",
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D15");
        }

        [Fact]
        public async Task Reject_when_product_is_invalid()
        {
            var request = Scenarios.CreateDocument()
                with
                {
                    ProductType = "Invalid_Method_Name",
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("E29");
        }

        [Fact]
        public async Task Reject_when_product_type_is_not_allowed_for_the_metering_point_type()
        {
            var request = Scenarios.CreateDocument()
                with
                {
                    ProductType = ProductType.Tariff.Name,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("E29");
        }

        [Fact]
        public async Task Should_reject_if_capacity_is_invalid()
        {
            var request = Scenarios.CreateDocument()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.One.Name,
                    PhysicalConnectionCapacity = "123.3333670",
                    MeteringMethod = MeteringMethod.Calculated.Name,
                    MeterNumber = null,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("E86");
        }

        [Fact]
        public async Task Should_reject_when_location_description_is_invalid()
        {
            var invalidLocationDescription = "1234567890123456789012345678901234567890123456789012345678901234567890";
            var request = Scenarios.CreateDocument()
                with
                {
                    LocationDescription = invalidLocationDescription,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("E86");
        }

        [Fact]
        public async Task Should_reject_if_asset_type_value_is_invalid()
        {
            var request = Scenarios.CreateDocument()
                with
                {
                    AssetType = "invalid_value",
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D59");
        }

        [Fact]
        public async Task Should_reject_when_country_code_is_not_dk()
        {
            var invalidCountryCode = "SE";
            var request = Scenarios.CreateDocument()
                with
                {
                    CountryCode = invalidCountryCode,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("E86");
        }

        [Fact]
        public async Task Effective_date_is_required()
        {
            var request = Scenarios.CreateDocument()
                with
                {
                    EffectiveDate = string.Empty,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D02");
        }

        [Fact]
        public async Task Metering_point_type_is_required()
        {
            var request = Scenarios.CreateDocument()
                with
                {
                    TypeOfMeteringPoint = string.Empty,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D02");
        }

        [Fact]
        public async Task Metering_point_type_must_be_valid()
        {
            var request = Scenarios.CreateDocument()
                with
                {
                    TypeOfMeteringPoint = "invalid value",
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D18");
        }

        [Fact]
        public async Task Should_reject_when_maximum_power_is_invalid()
        {
            var invalidPowerLimit = "12345567";
            var document = Scenarios.CreateDocument()
                with
                {
                    MaximumPower = invalidPowerLimit,
                };

            await Act.SendCommandAsync(document).ConfigureAwait(false);

            Assert.ValidationError("E86");
        }

        [Fact]
        public async Task Should_create_when_maximum_power_is_null()
        {
            var document = Scenarios.CreateDocument()
                with
                {
                    MaximumPower = null,
                };

            await Act.SendCommandAsync(document).ConfigureAwait(false);

            Assert.OutboxMessage<MessageHubEnvelope>(envelope => envelope.MessageType == DocumentType.ConfirmCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_create_when_maximum_current_is_null()
        {
            var document = Scenarios.CreateDocument()
                with
                {
                    MaximumCurrent = null,
                };

            await Act.SendCommandAsync(document).ConfigureAwait(false);

            Assert.OutboxMessage<MessageHubEnvelope>(envelope => envelope.MessageType == DocumentType.ConfirmCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_create_when_maximum_current_and_maximum_power_is_null()
        {
            var document = Scenarios.CreateDocument()
                with
                {
                    MaximumCurrent = null,
                    MaximumPower = null,
                };

            await Act.SendCommandAsync(document).ConfigureAwait(false);

            Assert.OutboxMessage<MessageHubEnvelope>(envelope => envelope.MessageType == DocumentType.ConfirmCreateMeteringPoint);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("invalid_value")]
        public async Task Should_reject_when_measurement_unit_is_missing_or_is_invalid(string measurementUnitType)
        {
            var request = Scenarios.CreateDocument()
                with
                {
                    MeasureUnitType = measurementUnitType,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("E73");
        }

        [Fact]
        public async Task Reject_when_unit_type_is_not_allowed_for_the_metering_point_type()
        {
            var request = Scenarios.CreateConsumptionMeteringPointCommand()
                with
                {
                    UnitType = MeasurementUnitType.Ampere.Name,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("E73");
        }

        [Fact]
        public async Task Should_reject_when_geo_info_reference_is_invalid()
        {
            var invalidGeoInfoReference = "xxxxxxx";
            var request = Scenarios.CreateDocument()
                with
                {
                    GeoInfoReference = invalidGeoInfoReference,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("E86");
        }

        [Fact]
        public async Task Should_reject_when_reading_occurence_is_not_a_valid_value()
        {
            var request = Scenarios.CreateDocument()
                with
                {
                    MeterReadingOccurrence = "Not_valid_Reading_occurence_value",
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D53");
        }

        [Fact]
        public async Task Should_reject_when_geo_info_reference_is_specified_and_official_address_is_empty()
        {
            var request = Scenarios.CreateDocument()
                with
                {
                    GeoInfoReference = SampleData.GeoInfoReference,
                    IsActualAddress = null,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("D63");
        }

        [Fact]
        public async Task Should_reject_when_maximum_current_is_invalid()
        {
            var invalidCurrent = "12345567";
            var request = Scenarios.CreateDocument()
                with
                {
                    MaximumCurrent = invalidCurrent,
                };

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            Assert.ValidationError("E86");
        }

        [Fact]
        public async Task Should_reject_when_current_actor_is_not_grid_operator_for_applied_grid_area()
        {
            Arrange.SetCurrentAuthenticatedActor(Guid.NewGuid());
            var request = Scenarios.CreateDocument();

            await Act.SendCommandAsync(request).ConfigureAwait(false);
            Assert.ValidationError("E0I");
        }

        protected override void AddEnvironmentVariables([NotNull] Dictionary<string, string> variables)
        {
            variables.Add("METERINGPOINT_DB_CONNECTION_STRING", DatabaseConnectionString);
        }

        protected override void OverrideRegistrations([NotNull] Container container)
        {
            // Fake current actor and user
            container.Register<IActorContext>(() => new ActorContext { CurrentActor = new Actor(SampleData.GridOperatorIdOfGrid870, "GLN", "8200000001409", "GridAccessProvider") }, Lifestyle.Singleton);
            container.Register<IUserContext>(() => new UserContext { CurrentUser = new User(Guid.NewGuid(), new List<Guid> { Guid.NewGuid() }) }, Lifestyle.Singleton);

            // Singleton for tests
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Singleton);

            // Specific for test instead of using Application Insights package
            container.Register(() => new TelemetryClient(new TelemetryConfiguration()), Lifestyle.Scoped);

            // Only for asserting process overview creation
            container.Register<IRequestHandler<MeteringPointProcessesByGsrnQuery, List<Process>>, MeteringPointProcessesByGsrnQueryHandler>();
        }

        private static CreateMeteringPoint CreateCommand()
        {
            return Scenarios.CreateConsumptionMeteringPointCommand();
        }
    }
}
