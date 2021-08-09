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
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation.Address
{
    [UnitTest]
    public class PostCodeRuleTests : CreateMeteringPointRulesTest<PostCodeRule>
    {
        [Theory]
        [InlineData("8000", nameof(MeteringPointType.Consumption), "DK")]
        [InlineData("POSTCODE", nameof(MeteringPointType.Consumption), "SE")]
        [InlineData("", "OtherType", "DK")]
        public void ShouldValidate(string postCode, string typeOfMeteringPoint, string countryCode)
        {
            var request = CreateRequest() with
            {
                StreetName = SampleData.StreetName,
                PostCode = postCode,
                CityName = SampleData.CityName,
                CountryCode = countryCode,
                GsrnNumber = SampleData.GsrnNumber,
                TypeOfMeteringPoint = typeOfMeteringPoint,
            };

            ShouldValidateWithNoErrors(request);
        }

        [Theory]
        [InlineData("800", nameof(MeteringPointType.Consumption), "DK", typeof(PostCodeWrongFormatValidationError))]
        [InlineData("", nameof(MeteringPointType.Consumption), "DK", typeof(PostCodeMandatoryForMeteringPointTypeValidationError))]
        [InlineData("LONGPOSTCODE", nameof(MeteringPointType.Consumption), "SE", typeof(PostCodeMaximumLengthValidationError))]
        public void ShouldResultInError(string postCode, string typeOfMeteringPoint, string countryCode, Type validationError)
        {
            var request = CreateRequest() with
            {
                StreetName = SampleData.StreetName,
                PostCode = postCode,
                CityName = SampleData.CityName,
                CountryCode = countryCode,
                GsrnNumber = SampleData.GsrnNumber,
                TypeOfMeteringPoint = typeOfMeteringPoint,
            };

            ShouldValidateWithSingleError(request, validationError);
        }
    }
}
