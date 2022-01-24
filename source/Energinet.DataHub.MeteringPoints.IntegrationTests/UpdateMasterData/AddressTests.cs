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
using System.Globalization;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class AddressTests : TestHost
    {
        public AddressTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Address_is_updated()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    StreetName = "New Street Name",
                    PostCode = "6000",
                    CityName = "New City Name",
                    StreetCode = "0500",
                    BuildingNumber = "4",
                    CitySubDivisionName = "New",
                    CountryCode = CountryCode.DK.Name,
                    FloorIdentification = "9",
                    RoomIdentification = "9",
                    MunicipalityCode = "999",
                    IsActualAddress = true,
                    GeoInfoReference = Guid.NewGuid().ToString(),
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertConfirmMessage(DocumentType.ConfirmChangeMasterData);
            AssertMasterData()
                .HasStreetName(request.StreetName)
                .HasPostCode(request.PostCode)
                .HasCity(request.CityName)
                .HasStreetCode(request.StreetCode)
                .HasBuildingNumber(request.BuildingNumber)
                .HasCitySubDivision(request.CitySubDivisionName)
                .HasCountryCode(CountryCode.DK)
                .HasFloor(request.FloorIdentification)
                .HasRoom(request.RoomIdentification)
                .HasMunicipalityCode(int.Parse(request.MunicipalityCode, CultureInfo.InvariantCulture))
                .HasIsActualAddress(request.IsActualAddress)
                .HasGeoInfoReference(Guid.Parse(request.GeoInfoReference));
        }

        [Fact]
        public async Task Reject_if_street_code_is_invalid()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    StreetCode = "Invalid value",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Reject_if_street_name_is_empty()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    StreetName = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Reject_if_street_name_is_too_long()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    StreetName = "AbcdefghijAbcdefghijAbcdefghijAbcdefghijAb",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Reject_if_building_number_is_invalid()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    BuildingNumber = "ABC1234",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Reject_if_floor_identification_is_invalid()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    FloorIdentification = "12345",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Reject_if_room_identification_is_invalid()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    RoomIdentification = "12345",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Reject_if_city_name_is_empty()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    CityName = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Reject_if_post_code_is_empty()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    PostCode = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }
    }
}
