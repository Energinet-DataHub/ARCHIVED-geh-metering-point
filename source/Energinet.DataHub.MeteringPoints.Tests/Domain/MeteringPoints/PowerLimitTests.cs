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

using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    [UnitTest]
    public class PowerLimitTests
        : TestBase
    {
        [Theory]
        [InlineData(1234567, true)]
        [InlineData(999999, false)]
        [InlineData(0, false)]
        public void Kilowat_is_limited_to_999999(int kwh, bool expectError)
        {
            var ampere = 999999;
            var checkResult = PowerLimit.CheckRules(kwh, ampere);

            AssertError<InvalidKwhPowerLimitRuleError>(checkResult, expectError);
        }

        [Theory]
        [InlineData(1234567, true)]
        [InlineData(999999, false)]
        [InlineData(0, false)]
        public void Ampere_is_limited_to_999999(int ampere, bool expectError)
        {
            var kwh = 999999;
            var checkResult = PowerLimit.CheckRules(kwh, ampere);

            AssertError<InvalidAmperePowerLimitRuleError>(checkResult, expectError);
        }

        [Fact]
        public void Should_create()
        {
            var kwh = 999999;
            var ampere = 999999;
            var powerLimit = PowerLimit.Create(kwh, ampere);

            Assert.NotNull(powerLimit);
            Assert.Equal(kwh, powerLimit.Kwh);
            Assert.Equal(ampere, powerLimit.Ampere);
        }

        [Fact]
        public void Should_throw_if_invalid_kwh_or_ampere()
        {
            var invalidKwh = 9999999;
            var invalidAmpere = 9999999;

            Assert.Throws<InvalidPowerLimitException>(() => PowerLimit.Create(invalidKwh, invalidAmpere));
        }
    }
}
