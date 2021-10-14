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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Events;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.Consumption
{
    public class ChangeMasterDataTests : TestBase
    {
        [Fact]
        public void Should_return_error_when_street_name_is_blank()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = new MasterDataDetails(string.Empty);

            var result = meteringPoint.CanChange(details);

            AssertError<StreetNameIsRequiredRuleError>(result, true);
        }

        [Fact]
        public void Should_return_success_when_street_name_is_null()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = new MasterDataDetails(null);

            var result = meteringPoint.CanChange(details);

            Assert.True(result.Success);
        }

        [Fact]
        public void Should_throw_if_any_business_rule_are_violated()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = new MasterDataDetails(string.Empty);

            Assert.Throws<MasterDataChangeException>(() => meteringPoint.Change(details));
        }

        [Fact]
        public void Should_change_street_name()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = new MasterDataDetails("New Street Name");

            meteringPoint.Change(details);

            var changeEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is MasterDataChanged) as MasterDataChanged;
            Assert.NotNull(changeEvent);
            Assert.Equal(details.StreetName, changeEvent?.StreetName);
        }

        private static ConsumptionMeteringPoint CreateMeteringPoint()
        {
            return ConsumptionMeteringPoint.Create(CreateDetails());
        }
    }
}
