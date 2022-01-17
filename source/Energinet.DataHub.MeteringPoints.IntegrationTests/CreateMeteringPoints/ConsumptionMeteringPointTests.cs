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
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Consumption;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CreateMeteringPoints
{
    [IntegrationTest]
    public class ConsumptionMeteringPointTests
        : TestHost
    {
        public ConsumptionMeteringPointTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Metering_point_is_created()
        {
            var request = CreateCommand();
            await SendCommandAsync(request).ConfigureAwait(false);

            await AssertMeteringPointExistsAsync(request.GsrnNumber).ConfigureAwait(false);
            AssertConfirmMessage(DocumentType.ConfirmCreateMeteringPoint);
            var integrationEvent = FindIntegrationEvent<ConsumptionMeteringPointCreatedIntegrationEvent>();
            Assert.NotNull(integrationEvent);
            Assert.Equal(request.GsrnNumber, integrationEvent?.GsrnNumber);
            Assert.Equal(request.MeteringMethod, integrationEvent?.MeteringMethod);
            Assert.Equal(request.SettlementMethod, integrationEvent?.SettlementMethod);
            Assert.Equal(request.MeteringGridArea, integrationEvent?.GridAreaCode);
            Assert.Equal(request.MeterReadingOccurrence, integrationEvent?.MeterReadingPeriodicity);
            Assert.Equal(request.NetSettlementGroup, integrationEvent?.NetSettlementGroup);
        }

        [Fact]
        public async Task Should_reject_when_powerplant_is_not_specified_and_netsettlementgroup_is_not_0_or_99()
        {
            var request = CreateCommand()
                with
                {
                    PowerPlant = string.Empty,
                    NetSettlementGroup = NetSettlementGroup.Six.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D57", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_street_name_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    StreetName = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_municipality_code_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    MunicipalityCode = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_street_code_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    StreetCode = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E00", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_building_number_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    BuildingNumber = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_disconnection_type_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    DisconnectionType = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_post_code_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    PostCode = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_city_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    CityName = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_reading_occurence_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    MeterReadingOccurrence = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_settlement_method_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    SettlementMethod = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_net_settlement_group_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    NetSettlementGroup = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_scheduled_meter_reading_date_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    ScheduledMeterReadingDate = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_day_of_scheduled_meter_reading_date_is_not_01_and_net_settlement_group_is_6()
        {
            var request = CreateCommand()
                with
                {
                    ScheduledMeterReadingDate = "0502",
                    NetSettlementGroup = NetSettlementGroup.Six.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_if_scheduled_meter_reading_date_is_specified_and_net_settlement_group_is_not_6()
        {
            var request = CreateCommand()
                with
                {
                    PowerPlant = SampleData.PowerPlantGsrnNumber,
                    NetSettlementGroup = NetSettlementGroup.One.Name,
                    ScheduledMeterReadingDate = "0101",
                    ConnectionType = ConnectionType.Installation.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_scheduled_meter_reading_date_is_invalid()
        {
            var request = CreateCommand()
                with
                {
                    ScheduledMeterReadingDate = "0631",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.RejectCreateMeteringPoint);
        }

        private static CreateMeteringPoint CreateCommand()
        {
            return Scenarios.CreateConsumptionMeteringPointCommand();
        }
    }
}
