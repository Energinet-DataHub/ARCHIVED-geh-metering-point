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
        [InlineData("0001", true)]
        [InlineData("0", false)]
        [InlineData("9999", true)]
        [InlineData("10000", false)]
        [InlineData("Abc", false)]
        public void Street_code_must_be_between_1_and_9999(string streetCode, bool isValid)
        {
            var checkResult = CheckRules(streetCode: streetCode);

            var hasError = checkResult.Errors.Any(error => error is StreetCodeLengthRuleError);
            Assert.Equal(isValid, !hasError);
        }

        [Fact]
        public void Street_code_can_be_empty()
        {
            var address = Address.Create(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

            Assert.Equal(string.Empty, address.StreetCode);
        }

        [Fact]
        public void Street_name_can_be_empty()
        {
            var address = Create();

            Assert.Equal(string.Empty, address.StreetName);
        }

        [Theory]
        [InlineData("Test street", true)]
        [InlineData("This street name is longer than 40 characters", false)]
        public void Street_name_should_be_max_40_characters_if_specified(string streetName, bool isValid)
        {
            var checkRules = CheckRules(streetName);

            var hasError = checkRules.Errors.Any(error => error is StreetNameLengthRuleError);
            Assert.Equal(isValid, !hasError);
        }

        private static Address Create(string? streetName = "", string? streetCode = "", string? cityName = "", string? postCode = "", string? countryCode = "")
        {
            return Address.Create(streetName, streetCode, cityName, postCode, countryCode);
        }

        private static BusinessRulesValidationResult CheckRules(string? streetName = "", string? streetCode = "")
        {
            return Address.CheckRules(streetName, streetCode);
        }
    }
}
