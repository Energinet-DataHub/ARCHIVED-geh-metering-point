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
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    [UnitTest]
    public class PowerLimitRuleTests : CreateMeteringPointRulesTest<PowerLimitRule>
    {
        [Theory]
        [InlineData("123456", null)]
        [InlineData("1", null)]
        [InlineData(null, "123456")]
        [InlineData(null, "1")]
        [InlineData("123456", "123456")]
        [InlineData("999999", "999999")]
        [InlineData("0", "0")]
        public void ShouldValidate(string contractedConnectionCapacity, string ratedCurrent)
        {
            var request = CreateRequest() with
            {
                ContractedConnectionCapacity = contractedConnectionCapacity,
                RatedCurrent = ratedCurrent,
                TypeOfMeteringPoint = MeteringPointType.Consumption.Name,
            };

            var errors = Validate(request);

            errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("123.456", null, typeof(KilowattPowerLimitValidationError))]
        [InlineData("1234567", null, typeof(KilowattPowerLimitValidationError))]
        [InlineData("-1", null, typeof(KilowattPowerLimitValidationError))]
        [InlineData(null, "123.456", typeof(AmperePowerLimitValidationError))]
        [InlineData(null, "1234567", typeof(AmperePowerLimitValidationError))]
        [InlineData(null, "-1", typeof(AmperePowerLimitValidationError))]
        [InlineData(null, "123N", typeof(AmperePowerLimitValidationError))]
        public void ShouldResultInError(string contractedConnectionCapacity, string ratedCurrent, Type expectedError)
        {
            var request = CreateRequest() with
            {
                ContractedConnectionCapacity = contractedConnectionCapacity,
                RatedCurrent = ratedCurrent,
                TypeOfMeteringPoint = MeteringPointType.Consumption.Name,
            };

            var errors = Validate(request);

            errors.Should().ContainSingle(error => error.GetType() == expectedError);
        }
    }
}
