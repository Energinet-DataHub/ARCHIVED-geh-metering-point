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

using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Validation;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    [UnitTest]
    public class CreateMeteringPointRuleSetTests
    {
        [Fact]
        public void Validate_WhenGsrnNumberIsEmpty_IsFailure()
        {
            var businessRequest = CreateRequest();

            var errors = GetValidationErrors(businessRequest);

            Assert.Contains(errors, error => error is GsrnNumberMustBeValidValidationError);
        }

        [Fact]
        public void Validate_WhenGsrnNumberIsNotFormattedCorrectly_IsFailure()
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = "Not_Valid_Gsrn_Number",
            };

            var errors = GetValidationErrors(businessRequest);

            Assert.Contains(errors, error => error is GsrnNumberMustBeValidValidationError);
        }

        [Theory]
        [InlineData("Consumption", "Flex", typeof(SettlementMethodRequiredValidationError), false)]
        [InlineData("NetLossCorrection", "Flex", typeof(SettlementMethodRequiredValidationError), false)]
        [InlineData("Consumption", "", typeof(SettlementMethodRequiredValidationError), true)]
        [InlineData("NetLossCorrection", "", typeof(SettlementMethodRequiredValidationError), true)]
        [InlineData("SettlementMethodNotAllowedForMP", "Flex", typeof(SettlementMethodNotAllowedValidationError), true)]
        [InlineData("SettlementMethodNotAllowedForMPEmpty", "", typeof(SettlementMethodNotAllowedValidationError), false)]
        [InlineData("Consumption", "WrongDomainName", typeof(SettlementMethodMissingRequiredDomainValuesValidationError), true)]
        public void Validate_MandatorySettlementMethodForConsumptionAndNetLossCorrectionMeteringType(string meteringPointType, string settlementMethod, System.Type errorType, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                TypeOfMeteringPoint = meteringPointType,
                SettlementMethod = settlementMethod,
            };

            ValidateCreateMeteringPoint(businessRequest, errorType, expectedError);
        }

        [Theory]
        [InlineData("***", typeof(MeteringGridAreaMandatoryValidationError), false)]
        [InlineData("", typeof(MeteringGridAreaMandatoryValidationError), true)]
        [InlineData("****", typeof(MeteringGridAreaLengthValidationError), true)]
        public void Validate_MeteringGridArea(string meteringGridArea, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                MeteringGridArea = meteringGridArea,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData("", typeof(OccurenceRequiredValidationError), true)]
        [InlineData("2021-11-12 12:12:12Z", typeof(OccurenceDateWrongFormatValidationError), false)]
        [InlineData("12-12-2021 12:12:12Z", typeof(OccurenceDateWrongFormatValidationError), true)]
        [InlineData("12-12-2021 12:12:12", typeof(OccurenceDateWrongFormatValidationError), true)]
        [InlineData("YYYY-12-12 12:12:12Z", typeof(OccurenceDateWrongFormatValidationError), true)]
        public void Validate_OccurenceDateMandatoryAndFormat(string occurenceDate, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                OccurenceDate = occurenceDate,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData("", typeof(MeteringPointTypeRequiredValidationError), true)]
        [InlineData("Consumption", typeof(MeteringPointTypeValidationError), false)]
        [InlineData("Production", typeof(MeteringPointTypeValidationError), false)]
        [InlineData("Exchange", typeof(MeteringPointTypeValidationError), false)]
        [InlineData("Unknown", typeof(MeteringPointTypeValidationError), true)]
        public void Validate_TypeOfMeteringPointRequiredAndInKnownType(string typeOfMeteringPoint, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                TypeOfMeteringPoint = typeOfMeteringPoint,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        private void ValidateCreateMeteringPoint(CreateMeteringPoint businessRequest, System.Type validationError, bool expectedError)
        {
            var errors = GetValidationErrors(businessRequest);
            var errorType = errors.Find(error => error.GetType() == validationError);

            if (!expectedError)
            {
                Assert.True(errorType == null);
            }
            else
            {
                Assert.Equal(validationError, errorType?.GetType());
            }
        }

        private CreateMeteringPoint CreateRequest()
        {
            return new CreateMeteringPoint(new Address());
        }

        private List<ValidationError> GetValidationErrors(CreateMeteringPoint request)
        {
            var ruleSet = new CreateMeteringPointRuleSet();
            var validationResult = ruleSet.Validate(request);

            return validationResult.Errors
                .Select(error => error.CustomState as ValidationError)
                .ToList();
        }
    }
}
