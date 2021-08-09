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
    public class BuildingNumberRuleTests : CreateMeteringPointRulesTest<BuildingNumberRule>
    {
        [Theory]
        [InlineData("22A", "DK")]
        [InlineData("AÆZ", "DK")]
        [InlineData("22ADA", "")]
        [InlineData("ÆØÅ", "")]
        [InlineData("1AAA", "")]
        public void ShouldValidate(string buildingNumber, string countryCode)
        {
            var request = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                CountryCode = countryCode,
                BuildingNumber = buildingNumber,
            };

            ShouldValidateWithNoErrors(request);
        }

        [Theory]
        [InlineData("", "DK", typeof(BuildingNumberMustBeValidValidationError))]
        [InlineData("001K", "DK", typeof(BuildingNumberMustBeValidValidationError))]
        [InlineData("001KA", "DK", typeof(BuildingNumberMustBeValidValidationError))]
        [InlineData("2AaIOAK", "", typeof(BuildingNumberMustBeValidValidationError))]
        public void ShouldResultInError(string buildingNumber, string countryCode, Type validationError)
        {
            var request = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                CountryCode = countryCode,
                BuildingNumber = buildingNumber,
            };

            ShouldValidateWithSingleError(request, validationError);
        }
    }
}
