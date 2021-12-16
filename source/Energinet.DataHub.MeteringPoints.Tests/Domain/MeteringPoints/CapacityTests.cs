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

using System.Globalization;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    public class CapacityTests : TestBase
    {
        [Theory]
        [InlineData("127.7777777", true)]
        [InlineData("1277777777", true)]
        [InlineData("ACBEE", true)]
        [InlineData("127777777", false)]
        [InlineData("1277.77777", false)]
        [InlineData("1277", false)]
        [InlineData("0", false)]
        public void Should_return_error_when_length_exceeds_9_digits(string capacity, bool expectError)
        {
            var checkResult = Capacity.CheckRules(capacity);

            AssertError<InvalidCapacityFormatRuleError>(checkResult, expectError);
        }

        [Fact]
        public void Should_throw_when_invalid()
        {
            var invalidCapacityValue = "1234567890";

            Assert.Throws<InvalidCapacityException>(() => Capacity.Create(invalidCapacityValue));
        }

        [Fact]
        public void Should_create()
        {
            string capacityValue = "123.000";
            var capacity = Capacity.Create(capacityValue);

            Assert.Equal(float.Parse(capacityValue, CultureInfo.InvariantCulture), capacity.Kw);
        }
    }
}
