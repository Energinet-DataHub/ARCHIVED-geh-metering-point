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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    public class CreateMeteringPointRuleSetTests
    {
        [Fact]
        public void Validate_WhenGsrnNumberIsEmpty_IsFailure()
        {
            var businessRequest = CreateRequest(string.Empty, string.Empty, string.Empty, string.Empty, 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, new Address(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

            var errors = GetValidationErrors(businessRequest);

            Assert.Contains(errors, error => error is GsrnNumberMustBeValidValidationError);
        }

        [Fact]
        public void Validate_WhenGsrnNumberIsNotFormattedCorrectly_IsFailure()
        {
            var businessRequest = CreateRequest("Not_Valid_Gsrn_Number", string.Empty, string.Empty, string.Empty, 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, new Address(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

            var errors = GetValidationErrors(businessRequest);

            Assert.Contains(errors, error => error is GsrnNumberMustBeValidValidationError);
        }

        [Theory]
        [InlineData("Consumption", "Flex", false)]
        [InlineData("NetLossCorrection", "Flex", false)]
        [InlineData("Consumption", "", true)]
        [InlineData("NetLossCorrection", "", true)]
        public void Validate_MandatorySettlementMethodForConsumptionAndNetLossCorrectionMeteringType(string meteringPointType, string settlementMethod, bool expectedError)
        {
            var businessRequest = CreateRequest(string.Empty, meteringPointType, string.Empty, string.Empty, 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, new Address(), string.Empty, settlementMethod, string.Empty, string.Empty, string.Empty, string.Empty);

            var errors = GetValidationErrors(businessRequest)
                .Where(error => error is SettlementMethodNotAllowedValidationError or SettlementMethodRequiredValidationError or SettlementMethodMissingRequiredDomainValuesValidationError);

            Assert.Equal(errors.Any(), expectedError);
        }

        private CreateMeteringPoint CreateRequest(
            string gsrnNumber,
            string typeOfMeteringPoint,
            string subTypeOfMeteringPoint,
            string meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            string meteringGridArea,
            string powerPlant,
            string locationDescription,
            string productType,
            Address installationLocationAddress,
            string parentRelatedMeteringPoint,
            string settlementMethod,
            string unitType,
            string disconnectionType,
            string occurenceDate,
            string meterNumber)
        {
            return new CreateMeteringPoint(
                gsrnNumber,
                typeOfMeteringPoint,
                subTypeOfMeteringPoint,
                meterReadingOccurrence,
                maximumCurrent,
                maximumPower,
                meteringGridArea,
                powerPlant,
                locationDescription,
                productType,
                installationLocationAddress,
                parentRelatedMeteringPoint,
                settlementMethod,
                unitType,
                disconnectionType,
                occurenceDate,
                meterNumber);
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
