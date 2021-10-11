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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CreateMeteringPoints
{
    [IntegrationTest]
    public class MarketMeteringPointTests : TestHost
    {
        public MarketMeteringPointTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Should_reject_if_meter_reading_occurrence_is_not_quarterly_or_hourly()
        {
            var invalidReadingOccurrence = ReadingOccurrence.Yearly.Name;
            var request = CreateRequest()
                with
                {
                    MeterReadingOccurrence = invalidReadingOccurrence,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_if_geo_info_reference_is_missing()
        {
            var request = CreateRequest()
                with
                {
                    GeoInfoReference = null,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_if_net_settlement_group_is_not_0_and_connection_type_is_undefined()
        {
            var request = CreateRequest()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.Six.Name,
                    ConnectionType = null,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_when_connection_type_does_not_match_net_settlement_group()
        {
            var request = CreateRequest()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.Six.Name,
                    ConnectionType = ConnectionType.Direct.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D55", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_if_metering_method_does_not_match_net_settlement_group()
        {
            var request = CreateRequest()
                with
                {
                    MeterNumber = "1",
                    NetSettlementGroup = NetSettlementGroup.Six.Name,
                    MeteringMethod = MeteringMethod.Physical.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D37", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_if_asset_type_is_required_and_not_specified()
        {
            var request = CreateRequest()
                with
                {
                    AssetType = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D59", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_if_asset_type_value_is_invalid()
        {
            var request = CreateRequest()
                with
                {
                    AssetType = "invalid_value",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D59", DocumentType.CreateMeteringPointRejected);
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
                MeteringMethod.Calculated.Name,
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
                null,
                Guid.NewGuid().ToString(),
                SampleData.PhysicalState,
                NetSettlementGroup.Six.Name,
                ConnectionType.Installation.Name,
                SampleData.AssetType,
                "123",
                ToGrid: "456",
                ParentRelatedMeteringPoint: null,
                SampleData.ProductType,
                "0",
                SampleData.GeoInfoReference,
                SampleData.MeasurementUnitType,
                "0101");
        }
    }
}
