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
using System.Collections;
using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Exceptions;
using Energinet.DataHub.MeteringPoints.Tests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    [UnitTest]
    public class EffectiveDateTests
    {
        [Theory]
        [InlineData("2021-06-01T22:00:00Z", true)]
        [InlineData("2021-12-30T23:00:00Z", true)]
        [InlineData("2021-06-01T22:00:00.000Z", true)]
        [InlineData("2021-06-01T22:00:00.100Z", false)]
        [InlineData("2021-06-01T23:01:00Z", false)]
        [InlineData("2021-06-01T00:00:00Z", false)]
        [InlineData("2021-06-01T00:00:00.000Z", false)]
        [InlineData("2021-09-25T22:00:00Z", true)]
        [InlineData("2021-06-01", false)]
        [InlineData("Not a date", false)]
        public void Date_format_must_be_able_to_handle_daylight_savings(string dateString, bool isValid)
        {
            var result = EffectiveDate.CheckRules(dateString);

            Assert.Equal(isValid, result.Success);
        }

        [Fact]
        public void Create_should_succeed_when_date_format_is_valid()
        {
            var dateString = TestHelpers.DaylightSavingsString(new DateTime(2021, 6, 1));
            var effectiveDate = EffectiveDate.Create(dateString);

            Assert.NotNull(effectiveDate);
            Assert.Equal(dateString, effectiveDate.ToString());
        }

        [Fact]
        public void Create_should_throw_exception_when_format_is_invalid()
        {
            var invalidDate = "2021-06-01";
            Assert.Throws<InvalidEffectiveDateFormat>(() => EffectiveDate.Create(invalidDate));
        }

        [Fact]
        public void Create_should_succeed_when_date_is_passed()
        {
            var date = TestHelpers.DaylightSavingsAdjusted(DateTime.Now);
            var effectiveDate = EffectiveDate.Create(date);

            Assert.NotNull(effectiveDate);
            Assert.Equal(date, effectiveDate.DateInUtc.ToDateTimeUtc());
        }
    }
}
