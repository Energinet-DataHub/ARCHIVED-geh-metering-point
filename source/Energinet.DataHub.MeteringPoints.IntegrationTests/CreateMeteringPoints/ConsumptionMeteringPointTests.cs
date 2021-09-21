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
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using MediatR;
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
        public async Task Should_reject_when_powerplant_is_not_specified_and_netsettlementgroup_is_not_0_or_99()
        {
            var request = CreateRequest()
                with
                {
                    PowerPlant = string.Empty,
                    NetSettlementGroup = NetSettlementGroup.Six.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D57");
        }

        [Fact]
        public async Task Should_reject_when_street_name_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    StreetName = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("E86");
        }

        [Fact]
        public async Task Should_reject_when_post_code_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    PostCode = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("E86");
        }

        [Fact]
        public async Task Should_reject_when_city_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    CityName = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("E86");
        }

        [Fact]
        public async Task Should_reject_when_reading_occurence_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    MeterReadingOccurrence = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D02");
        }

        [Fact]
        public async Task Should_reject_when_reading_occurence_is_not_a_valid_value()
        {
            var request = CreateRequest()
                with
                {
                    MeterReadingOccurrence = "Not_valid_Reading_occurence_value",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D02");
        }

        [Fact]
        public async Task Should_reject_when_settlement_method_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    SettlementMethod = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D02");
        }

        [Fact]
        public async Task Should_reject_when_settlement_method_is_invalid()
        {
            var request = CreateRequest()
                with
                {
                    SettlementMethod = "Invalid_Method_Name",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D15");
        }

        [Fact]
        public async Task Should_reject_when_net_settlement_group_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    NetSettlementGroup = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D02");
        }

        [Fact]
        public async Task Should_reject_when_net_settlement_group_is_invalid()
        {
            var request = CreateRequest()
                with
                {
                    NetSettlementGroup = "Invalid_netsettlement_group_value",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D02");
        }

        [Fact]
        public async Task Should_reject_when_scheduled_meter_reading_date_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    ScheduledMeterReadingDate = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D02");
        }

        [Fact]
        public async Task Should_reject_when_day_of_scheduled_meter_reading_date_is_not_01_and_net_settlement_group_is_6()
        {
            var request = CreateRequest()
                with
                {
                    ScheduledMeterReadingDate = "0502",
                    NetSettlementGroup = NetSettlementGroup.Six.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D02");
        }

        [Fact]
        public async Task Should_reject_if_scheduled_meter_reading_date_is_specified_and_net_settlement_group_is_not_6()
        {
            var request = CreateRequest()
                with
                {
                    PowerPlant = SampleData.PowerPlantGsrnNumber,
                    NetSettlementGroup = NetSettlementGroup.One.Name,
                    ScheduledMeterReadingDate = "0101",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D02");
        }

        [Fact]
        public async Task Should_reject_when_scheduled_meter_reading_date_is_invalid()
        {
            var request = CreateRequest()
                with
                {
                    ScheduledMeterReadingDate = "0631",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("E86");
        }

        [Fact]
        public async Task Should_reject_when_measurement_unit_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    MeasureUnitType = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError<CreateMeteringPointRejected>("D02");
        }

        private static CreateMeteringPoint CreateRequest()
        {
            return new CreateMeteringPoint(
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
                MeteringPointSubType.Calculated.Name,
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
                string.Empty,
                Guid.NewGuid().ToString(),
                SampleData.PhysicalState,
                NetSettlementGroup.Six.Name,
                SampleData.ConnectionType,
                SampleData.AssetType,
                "123",
                ToGrid: "456",
                ParentRelatedMeteringPoint: null,
                SampleData.ProductType,
                "0",
                SampleData.GeoInfoReference,
                SampleData.MeasurementUnitType,
                ContractedConnectionCapacity: null,
                RatedCurrent: null,
                "0101");
        }
    }
}
