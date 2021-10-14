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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    [UnitTest]
    public class CreateTests : TestBase
    {
        [Fact]
        public void Should_return_error_when_subtype_is_physical_and_meter_id_is_undefined()
        {
            var details = CreateConsumptionDetails()
                with
                {
                    MeterNumber = null, MeteringMethod = MeteringMethod.Physical,
                };

            var result = MeteringPoint.CanCreate(details);

            AssertError<MeterIdIsRequiredRuleError>(result, true);
        }

        [Fact]
        public void Should_return_error_when_subtype_is_not_and_meter_id_is_defined()
        {
            var details = CreateConsumptionDetails()
                with
                {
                    MeterNumber = MeterId.Create("A1234"),
                    MeteringMethod = MeteringMethod.Virtual,
                };

            var result = MeteringPoint.CanCreate(details);

            AssertError<MeterIdIsNotAllowedRuleError>(result, true);
        }
    }
}
