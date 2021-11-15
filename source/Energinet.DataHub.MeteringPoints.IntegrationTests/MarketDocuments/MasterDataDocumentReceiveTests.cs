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
