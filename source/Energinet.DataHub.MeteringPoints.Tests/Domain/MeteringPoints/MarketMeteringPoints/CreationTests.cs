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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.MarketMeteringPoints
{
    [UnitTest]
    public class CreationTests : TestBase
    {
        [Fact]
        public void Should_return_error__meter_reading_occurrence_is_not_quarterly_or_hourly()
        {
            var details = CreateDetails()
                with
                {
                    ReadingOccurrence = ReadingOccurrence.Yearly,
                };

            var result = MarketMeteringPoint.CanCreate(details);

            Assert.False(result.Success);
            AssertError<InvalidMeterReadingOccurrenceRuleError>(result, true);
        }

        [Fact]
        public void Connection_type_is_required_when_net_settlement_group_is_not_0()
        {
            var details = CreateDetails()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.Six,
                    ConnectionType = null,
                };

            var result = MarketMeteringPoint.CanCreate(details);

            Assert.False(result.Success);
            AssertError<ConnectionTypeIsRequiredRuleError>(result, true);
        }
    }
}
