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
                countryCode: "DK");

            AssertError<BuildingNumberFormatRuleError>(checkResult, expectError);
        }

        [Theory]
        [InlineData("123456", "", false)]
        [InlineData("123456", "SE", false)]
        [InlineData("1234567", "", true)]
        [InlineData("1234567", "SE", true)]
        public void Building_number_lenght_should_be_6_characters(string buildingNumber, string countryCode, bool expectError)
        {
            var checkResult = CheckRules(
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: buildingNumber,
                countryCode: countryCode);

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
                countryCode: string.Empty,
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
                countryCode: string.Empty,
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
                countryCode: string.Empty,
                floor: string.Empty,
                room: string.Empty);

            AssertError<CityNameLengthRuleError>(checkResult, expectError);
        }

        private static Address Create(string streetName = "", string streetCode = "", string buildingNumber = "", string cityName = "", string postCode = "", string countryCode = "", string floor = "", string room = "")
        {
            return Address.Create(streetName, streetCode, buildingNumber, postCode, cityName, countryCode, floor, room);
        }

        private static BusinessRulesValidationResult CheckRules(string? streetName = "", string? streetCode = "", string? buildingNumber = "", string? city = "", string? countryCode = "", string? floor = "", string? room = "")
        {
            return Address.CheckRules(
                streetName: streetName,
                streetCode: streetCode,
                buildingNumber: buildingNumber,
                city: city,
                countryCode: countryCode,
                floor: floor,
                room: room);
        }

        private static void AssertError<TRuleError>(BusinessRulesValidationResult rulesValidationResult, bool errorExpected)
        {
            var hasError = rulesValidationResult.Errors.Any(error => error is TRuleError);
            Assert.Equal(errorExpected, hasError);
        }
    }
}
