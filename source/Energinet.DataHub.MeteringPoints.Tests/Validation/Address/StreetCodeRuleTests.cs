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
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation.Address
{
    [UnitTest]
    public class StreetCodeRuleTests : CreateMeteringPointRulesTest<StreetCodeRule>
    {
        [Theory]
        [InlineData("1234")]
        [InlineData("0001")]
        [InlineData("9999")]
        public void ShouldValidate(string streetCode)
        {
            var request = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                StreetCode = streetCode,
            };

            ShouldValidateWithNoErrors(request);
        }

        [Theory]
        [InlineData("0000", typeof(StreetCodeValidationError))]
        [InlineData("00011", typeof(StreetCodeValidationError))]
        [InlineData("12345", typeof(StreetCodeValidationError))]
        [InlineData("9", typeof(StreetCodeValidationError))]
        [InlineData("afsd", typeof(StreetCodeValidationError))]
        public void ShouldResultInError(string streetCode, Type validationError)
        {
            var request = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                StreetCode = streetCode,
            };

            ShouldValidateWithSingleError(request, validationError);
        }
    }
}
