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

using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    [UnitTest]
    public class MeterIdTests : TestBase
    {
        [Fact]
        public void Should_return_error_when_value_exceeds_15_characters()
        {
            var invalidMeterId = "12345678901234567890";
            var checkResult = MeterId.CheckRules(invalidMeterId);

            AssertError<InvalidMeterIdRuleError>(checkResult, true);
        }

        [Fact]
        public void Should_throw_when_invalid()
        {
            var invalidMeterId = "A12345678901234567890";
            Assert.Throws<InvalidMeterIdException>(() => MeterId.Create(invalidMeterId));
        }

        [Fact]
        public void Should_create()
        {
            var meterIdValue = "A1234";
            var meterId = MeterId.Create(meterIdValue);

            Assert.Equal(meterIdValue, meterId.Value);
        }
    }
}
