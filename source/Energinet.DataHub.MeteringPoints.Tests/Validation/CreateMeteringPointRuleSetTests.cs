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
using System.Reflection;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Validation;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
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
        [InlineData("Consumption", "Flex", (SettlementMethodRequiredValidationError)null)]
        [InlineData("NetLossCorrection", "Flex", (SettlementMethodRequiredValidationError)null)]
        [InlineData("Consumption", "", typeof(SettlementMethodRequiredValidationError))]
        [InlineData("NetLossCorrection", "", typeof(SettlementMethodRequiredValidationError))]
        [InlineData("SettlementMethodNotAllowedForMP", "Flex", typeof(SettlementMethodNotAllowedValidationError))]
        [InlineData("SettlementMethodNotAllowedForMPEmpty", "", (SettlementMethodNotAllowedValidationError)null)]
        [InlineData("Consumption", "WrongDomainName", typeof(SettlementMethodMissingRequiredDomainValuesValidationError))]
        public void Validate_MandatorySettlementMethodForConsumptionAndNetLossCorrectionMeteringType(string meteringPointType, string settlementMethod, System.Type expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                TypeOfMeteringPoint = meteringPointType,
                SettlementMethod = settlementMethod,
            };

            var errorType = GetValidationErrors(businessRequest).FirstOrDefault(error => error.GetType() == expectedError);

            Assert.Equal(expectedError, errorType?.GetType());
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
