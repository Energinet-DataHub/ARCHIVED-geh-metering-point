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
using Energinet.DataHub.MeteringPoints.Domain.Policies;
using Energinet.DataHub.MeteringPoints.Tests.Tooling;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.Policies
{
    [UnitTest]
    public class EffectiveDatePolicyTests : TestBase
    {
        [Theory]
        [InlineData("2020-12-26T22:00:00Z", 5, true)]
        [InlineData("2020-12-26T22:00:00Z", 10, false)]
        public void Effective_date_is_within_range_of_allowed_number_of_days_before_today(string effectiveDate, int allowedNumberOfDaysBeforeToday, bool expectError)
        {
            const string todayDate = "2021-01-01T11:00:00Z";
            var policy = new EffectiveDatePolicy(allowedNumberOfDaysBeforeToday, 0);
            var today = InstantPattern.General.Parse(todayDate).Value;
            var effective = EffectiveDate.Create(effectiveDate);

            var result = policy.Check(today, effective);

            AssertError<EffectiveDateIsNotWithinAllowedTimePeriod>("E17", result, expectError);
        }

        [Theory]
        [InlineData("2021-01-10T22:00:00Z", 5, true)]
        [InlineData("2021-01-10T22:00:00Z", 10, false)]
        public void Effective_date_is_within_range_of_allowed_number_of_days_after_today(string effectiveDate, int allowedNumberOfDaysAfterToday, bool expectError)
        {
            const string todayDate = "2021-01-01T11:00:00Z";
            var policy = new EffectiveDatePolicy(0, allowedNumberOfDaysAfterToday);
            var today = InstantPattern.General.Parse(todayDate).Value;
            var effective = EffectiveDate.Create(effectiveDate);

            var result = policy.Check(today, effective);

            AssertError<EffectiveDateIsNotWithinAllowedTimePeriod>("E17", result, expectError);
        }

        [Fact]
        public void Same_date_is_allowed()
        {
            var policy = new EffectiveDatePolicy(10, 10);
            var today = InstantPattern.General.Parse("2021-01-10T10:00:00Z").Value;
            var effective = EffectiveDate.Create("2021-01-10T22:00:00Z");

            var result = policy.Check(today, effective);

            AssertError<EffectiveDateIsNotWithinAllowedTimePeriod>("E17", result, false);
        }
    }
}
