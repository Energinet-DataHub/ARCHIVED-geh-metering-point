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
    public class FloorIdentificationRuleTests : CreateMeteringPointRulesTest<FloorIdentificationRule>
    {
        [Theory]
        [InlineData("")]
        [InlineData("2")]
        [InlineData("A")]
        public void ShouldValidate(string floorIdentification)
        {
            var request = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                FloorIdentification = floorIdentification,
            };

            ShouldValidateWithNoErrors(request);
        }

        [Theory]
        [InlineData("ABCDE", typeof(FloorIdentificationValidationError))]
        [InlineData("12345", typeof(FloorIdentificationValidationError))]
        public void ShouldResultInError(string floorIdentification, Type validationError)
        {
            var request = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                FloorIdentification = floorIdentification,
            };

            ShouldValidateWithSingleError(request, validationError);
        }
    }
}
