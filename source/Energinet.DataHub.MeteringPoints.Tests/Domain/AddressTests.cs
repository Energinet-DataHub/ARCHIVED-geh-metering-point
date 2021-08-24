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

using System.Linq;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
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
            var checkResult = Address.CheckRules(streetCode);

            var hasError = checkResult.Errors.Any(error => error is InvalidStreetCodeError);
            Assert.Equal(isValid, !hasError);
        }

        [Fact]
        public void Street_code_should_consist_of_exactly_4_characters()
        {
            var streetName = "Fake";
            var streetCode = "0001";
            var cityName = "Fake";
            var postCode = "Fake";
            var countryCode = "Fake";

            var address = Address.Create(streetName, streetCode, cityName, postCode, countryCode);

            Assert.Equal("0001", address.StreetCode);
        }
    }
}
