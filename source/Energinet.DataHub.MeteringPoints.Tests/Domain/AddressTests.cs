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
using System.Linq;
using System.Text;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain
{
    [UnitTest]
    public class AddressTests
    {
        [Theory]
        [InlineData("0001", false)]
        [InlineData("0", true)]
        [InlineData("9999", false)]
        [InlineData("10000", true)]
        [InlineData("Abc", true)]
        public void Street_code_must_be_between_1_and_9999(string streetCode, bool expectError)
        {
            var checkResult = CheckRules(streetCode: streetCode);

            AssertError<StreetCodeLengthRuleError>(checkResult, expectError);
        }

        [Fact]
        public void Street_code_can_be_empty()
        {
            var address = Create();

            Assert.Equal(string.Empty, address.StreetCode);
        }

        [Fact]
        public void Street_name_can_be_empty()
        {
            var address = Create();

            Assert.Equal(string.Empty, address.StreetName);
        }

        [Theory]
        [InlineData("Test street", false)]
        [InlineData("This street name is longer than 40 characters", true)]
        public void Street_name_should_be_max_40_characters_if_specified(string streetName, bool errorExpected)
        {
            var checkResult = CheckRules(streetName);

            AssertError<StreetNameLengthRuleError>(checkResult, errorExpected);
        }

        [Theory]
        [InlineData("1", false)]
        [InlineData("11", false)]
        [InlineData("999", false)]
        [InlineData("999A", false)]
        [InlineData("ABCD", false)]
        [InlineData("1000", true)]
        [InlineData("ABCDE", true)]
        [InlineData("A BE", true)]
        public void Building_number_format_is_restricted_when_country_code_is_DK(string buildingNumber, bool expectError)
        {
            var checkResult = CheckRules(
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: buildingNumber,
                countryCode: CountryCode.DK);

            AssertError<BuildingNumberFormatRuleError>(checkResult, expectError);
        }

        [Theory]
        [InlineData("123456", "", false)]
        [InlineData("1234567", "", true)]
        public void Building_number_lenght_should_be_6_characters(string buildingNumber, string countryCode, bool expectError)
        {
            var checkResult = CheckRules(
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: buildingNumber,
                countryCode: !string.IsNullOrWhiteSpace(countryCode) ? EnumerationType.FromName<CountryCode>(countryCode) : null);

            AssertError<BuildingNumberFormatRuleError>(checkResult, expectError);
        }

        [Theory]
        [InlineData("1", false)]
        [InlineData("", false)]
        [InlineData("ABCD", false)]
        [InlineData("ABCDE", true)]
        public void Floor_must_not_exceed_4_characters(string floor, bool expectError)
        {
            var checkResult = CheckRules(
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: string.Empty,
                countryCode: null,
                floor: floor);

            AssertError<FloorLengthRuleError>(checkResult, expectError);
        }

        [Theory]
        [InlineData("1", false)]
        [InlineData("", false)]
        [InlineData("ABCD", false)]
        [InlineData("ABCDE", true)]
        public void Room_must_not_exceed_4_characters(string room, bool expectError)
        {
            var checkResult = CheckRules(
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: string.Empty,
                countryCode: null,
                floor: string.Empty,
                room: room);

            AssertError<RoomLengthRuleError>(checkResult, expectError);
        }

        [Theory]
        [InlineData("A", false)]
        [InlineData("", false)]
        [InlineData("Test city", false)]
        [InlineData("The name of this city is just too long", true)]
        public void City_name_must_not_exceed_25_characters(string city, bool expectError)
        {
            var checkResult = CheckRules(
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: string.Empty,
                city: city,
                countryCode: null,
                floor: string.Empty,
                room: string.Empty);

            AssertError<CityNameLengthRuleError>(checkResult, expectError);
        }

        [Theory]
        [InlineData("0000", false)]
        [InlineData("9999", false)]
        [InlineData("6690", false)]
        [InlineData("11", true)]
        [InlineData("ABE", true)]
        public void Post_code_should_be_restricted_to_0000_to_9999_when_country_code_is_DK(string postCode, bool expectError)
        {
            var checkResult = CheckRules(
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: string.Empty,
                city: string.Empty,
                postCode: postCode,
                countryCode: CountryCode.DK,
                floor: string.Empty,
                room: string.Empty);

            AssertError<PostCodeFormatRuleError>(checkResult, expectError);
        }

        [Theory]
        [InlineData("0000", false)]
        [InlineData("9999", false)]
        [InlineData("6690", false)]
        [InlineData("1111111111", false)]
        [InlineData("11111111111", true)]
        public void Post_code_should_be_restricted_to_10_characters(string postCode, bool expectError)
        {
            var checkResult = CheckRules(
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: string.Empty,
                city: string.Empty,
                postCode: postCode,
                countryCode: null,
                floor: string.Empty,
                room: string.Empty);

            AssertError<PostCodeFormatRuleError>(checkResult, expectError);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("0000", false)]
        [InlineData("9999", false)]
        [InlineData("6690", false)]
        [InlineData("1111111111", false)]
        [InlineData("12345678901234567890123456789012345", true)]
        public void City_sub_division_should_be_restricted_to_max_34_characters(string citySubDivision, bool expectError)
        {
            var checkResult = CheckRules(
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: string.Empty,
                city: string.Empty,
                citySubDivision: citySubDivision,
                postCode: string.Empty,
                countryCode: null,
                floor: string.Empty,
                room: string.Empty);

            AssertError<CitySubdivisionRuleError>(checkResult, expectError);
        }

        [Theory]
        [InlineData(100, false)]
        [InlineData(999, false)]
        [InlineData(250, false)]
        [InlineData(1000, true)]
        [InlineData(50, true)]
        [InlineData(0, false)]
        public void Municipality_code_should_be_3_digits_from_100_999(int municipalityCode, bool expectError)
        {
            var checkResult = CheckRules(
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: string.Empty,
                city: string.Empty,
                citySubDivision: string.Empty,
                postCode: string.Empty,
                countryCode: null,
                floor: string.Empty,
                room: string.Empty,
                municipalityCode: municipalityCode);

            AssertError<MunicipalityCodeRuleError>(checkResult, expectError);
        }

        [Fact]
        public void Should_create()
        {
            var streetName = "Testing Street";
            var streetCode = "0005";
            var buildingNumber = "10";
            var city = "Test City";
            var citySubdivision = "Test subdivision";
            var postCode = "9000";
            var countryCode = CountryCode.DK;
            var floor = "1";
            var room = "tv";
            var municipalityCode = 100;
            var isActual = true;
            var geoInfoReference = Guid.Parse("5B736036-7612-4350-A73D-058560350E32");
            var locationDescription = "Test location";

            var address = Create(
                streetName: streetName,
                streetCode: streetCode,
                buildingNumber: buildingNumber,
                city: city,
                citySubDivision: citySubdivision,
                postCode: postCode,
                countryCode: countryCode,
                floor: floor,
                room: room,
                municipalityCode: municipalityCode,
                isActual: isActual,
                geoInfoReference: geoInfoReference,
                locationDescription: locationDescription);

            Assert.Equal(streetName, address.StreetName);
            Assert.Equal(streetCode, address.StreetCode);
            Assert.Equal(buildingNumber, address.BuildingNumber);
            Assert.Equal(city, address.City);
            Assert.Equal(citySubdivision, address.CitySubDivision);
            Assert.Equal(postCode, address.PostCode);
            Assert.Equal(countryCode, address.CountryCode);
            Assert.Equal(floor, address.Floor);
            Assert.Equal(room, address.Room);
            Assert.Equal(municipalityCode, address.MunicipalityCode);
            Assert.Equal(geoInfoReference, address.GeoInfoReference);
            Assert.Equal(locationDescription, address.LocationDescription);
        }

        [Fact]
        public void Null_values_are_ignored_and_existing_values_preserved()
        {
            var originalAddress = Address.Create(
                streetName: "Original Street Name",
                countryCode: null,
                city: "Original City",
                streetCode: null,
                buildingNumber: null,
                citySubDivision: null,
                postCode: null,
                floor: null,
                room: null,
                municipalityCode: null,
                isActual: false,
                geoInfoReference: null);

            var updatedAddress = Address.Create(
                streetName: "Updated Street Name",
                countryCode: CountryCode.DK,
                city: null,
                streetCode: null,
                buildingNumber: null,
                citySubDivision: null,
                postCode: null,
                floor: null,
                room: null,
                municipalityCode: null,
                isActual: false,
                geoInfoReference: null);

            var mergedAddress = originalAddress.MergeFrom(updatedAddress);

            Assert.Equal(updatedAddress.StreetName, mergedAddress.StreetName);
            Assert.Equal(updatedAddress.CountryCode, mergedAddress.CountryCode);
            Assert.Equal(originalAddress.City, mergedAddress.City);
        }

        [Fact]
        public void Location_description_is_restricted_to_60_characters()
        {
            var invalidDescription = "1234567890123456789012345678901234567890123456789012345678901234567890";
            var result = CheckRules(locationDescription: invalidDescription);

            AssertError<InvalidLocationDescriptionRuleError>(result, true);
        }

        private static Address Create(
            string streetName = "",
            string streetCode = "",
            string buildingNumber = "",
            string city = "",
            string citySubDivision = "",
            string postCode = "",
            CountryCode? countryCode = null,
            string floor = "",
            string room = "",
            int municipalityCode = default(int),
            bool isActual = false,
            Guid? geoInfoReference = null,
            string? locationDescription = null)
        {
            return Address.Create(
               streetName: streetName,
               streetCode: streetCode,
               buildingNumber: buildingNumber,
               city: city,
               citySubDivision: citySubDivision,
               postCode: postCode,
               countryCode: countryCode,
               floor: floor,
               room: room,
               municipalityCode: municipalityCode,
               isActual: isActual,
               geoInfoReference: geoInfoReference,
               locationDescription: locationDescription);
        }

        private static BusinessRulesValidationResult CheckRules(string? streetName = "", string? streetCode = "", string? buildingNumber = "", string? city = "", string? citySubDivision = "", string? postCode = "", CountryCode? countryCode = null, string? floor = "", string room = "", int municipalityCode = default(int), string locationDescription = "")
        {
            return Address.CheckRules(
                streetName: streetName,
                streetCode: streetCode,
                buildingNumber: buildingNumber,
                city: city,
                citySubDivision: citySubDivision,
                postCode: postCode,
                countryCode: countryCode,
                floor: floor,
                room: room,
                municipalityCode: municipalityCode,
                locationDescription: locationDescription);
        }

        private static void AssertError<TRuleError>(BusinessRulesValidationResult rulesValidationResult, bool errorExpected)
        {
            var hasError = rulesValidationResult.Errors.Any(error => error is TRuleError);
            Assert.Equal(errorExpected, hasError);
        }
    }
}
