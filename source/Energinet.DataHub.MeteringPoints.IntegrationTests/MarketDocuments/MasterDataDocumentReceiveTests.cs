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
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.MarketDocuments
{
    [IntegrationTest]
    public class MasterDataDocumentReceiveTests : TestHost
    {
        public MasterDataDocumentReceiveTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Should_reject_when_country_code_is_not_dk()
        {
            var invalidCountryCode = "SE";
            var request = CreateDocument()
                with
                {
                    CountryCode = invalidCountryCode,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Should_reject_when_net_settlement_group_is_invalid()
        {
            var request = CreateDocument()
                with
                {
                    NetSettlementGroup = "Invalid_netsettlement_group_value",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02");
        }

        [Fact]
        public async Task Should_reject_when_maximum_power_is_invalid()
        {
            var invalidPowerLimit = 12345567;
            var document = CreateDocument()
                with
                {
                    MaximumPower = invalidPowerLimit,
                };

            await SendCommandAsync(document).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Should_reject_when_maximum_current_is_invalid()
        {
            var invalidCurrent = 12345567;
            var request = CreateDocument()
                with
                {
                    MaximumCurrent = invalidCurrent,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Should_reject_when_location_description_is_invalid()
        {
            var invalidLocationDescription = "1234567890123456789012345678901234567890123456789012345678901234567890";
            var request = CreateDocument()
                with
                {
                    LocationDescription = invalidLocationDescription,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Should_reject_if_capacity_is_invalid()
        {
            var request = CreateDocument()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.One.Name,
                    PhysicalConnectionCapacity = "123.3333670",
                    MeteringMethod = MeteringMethod.Calculated.Name,
                    MeterNumber = null,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Should_reject_when_geo_info_reference_is_invalid()
        {
            var invalidGeoInfoReference = "xxxxxxx";
            var request = CreateDocument()
                with
                {
                    GeoInfoReference = invalidGeoInfoReference,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Should_reject_when_geo_info_reference_is_specified_and_official_address_is_empty()
        {
            var request = CreateDocument()
                with
                {
                    GeoInfoReference = SampleData.GeoInfoReference,
                    IsActualAddress = null,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D63");
        }

        [Fact]
        public async Task Should_reject_if_connection_type_is_unknown()
        {
            var invalidConnectionType = "invalid_value";
            var request = CreateDocument()
                with
                {
                    ConnectionType = invalidConnectionType,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02");
        }

        [Fact]
        public async Task Should_reject_when_measurement_unit_is_missing()
        {
            var request = CreateDocument()
                with
                {
                    MeasureUnitType = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("invalid_value")]
        public async Task Should_reject_when_measurement_unit_is_missing_or_is_invalid(string measurementUnitType)
        {
            var request = CreateDocument()
                with
                {
                    MeasureUnitType = measurementUnitType,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02");
        }

        [Fact]
        public async Task Should_reject_when_reading_occurence_is_not_a_valid_value()
        {
            var request = CreateDocument()
                with
                {
                    MeterReadingOccurrence = "Not_valid_Reading_occurence_value",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02");
        }

        [Fact]
        public async Task Should_reject_when_settlement_method_is_invalid()
        {
            var request = CreateDocument()
                with
                {
                    SettlementMethod = "Invalid_Method_Name",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D15");
        }

        [Fact]
        public async Task Should_reject_when_disconnection_type_is_invalid()
        {
            var request = CreateDocument()
                with
                {
                    TypeOfMeteringPoint = nameof(MeteringPointType.Production),
                    DisconnectionType = "invalid_dc_type",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02");
        }

        [Fact]
        public async Task Should_reject_when_metering_method_is_invalid()
        {
            var request = CreateDocument()
                with
                {
                    MeteringMethod = "Invalid_value",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02");
        }

        [Fact]
        public async Task Should_reject_if_asset_type_value_is_invalid()
        {
            var request = CreateDocument()
                with
                {
                    AssetType = "invalid_value",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D59");
        }

        private static MasterDataDocument CreateDocument()
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
                SampleData.IsActualAddress,
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
