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

using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Consumption.Rules;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.Consumption
{
    public class ScheduledMeterReadingDateTests : TestBase
    {
        [Theory]
        [InlineData("AAAA", true)]
        [InlineData("1331", true)]
        [InlineData("12311", true)]
        [InlineData("0031", true)]
        [InlineData("0631", true)]
        [InlineData("01", true)]
        [InlineData("01A", true)]
        [InlineData("0S1", true)]
        [InlineData("1404", true)]
        [InlineData("1D404", true)]
        [InlineData("0101D", true)]
        [InlineData("0630", false)]
        public void Should_return_validation_error_when_month_day_is_invalid(string monthAndDay, bool expectError)
        {
            var checkResult = ScheduledMeterReadingDate.CheckRules(monthAndDay);

            AssertError<InvalidScheduledMeterReadingDateRuleError>("E86", checkResult, expectError);
        }

        [Fact]
        public void Should_throw_when_month_and_day_is_invalid()
        {
            var invalidMonthAndDay =
                Assert.Throws<InvalidScheduledMeterReadingDateException>(() => ScheduledMeterReadingDate.Create("1350"));
        }

        [Fact]
        public void Should_create()
        {
            var monthAndDay = "0201";
            var scheduledMeterReadingDate = ScheduledMeterReadingDate.Create(monthAndDay);

            Assert.Equal(monthAndDay, scheduledMeterReadingDate.MonthAndDay);
            Assert.Equal("01", scheduledMeterReadingDate.Day);
        }
    }
}
