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
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    [UnitTest]
    public class CreateMeteringPointRuleSetTests
    {
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
        [InlineData("", typeof(MeteringPointTypeRequiredValidationError), true)]
        [InlineData("Consumption", typeof(MeteringPointTypeValidationError), false)]
        [InlineData("Production", typeof(MeteringPointTypeValidationError), false)]
        [InlineData("Exchange", typeof(MeteringPointTypeValidationError), false)]
        [InlineData("Unknown", typeof(MeteringPointTypeValidationError), true)]
        public void Validate_TypeOfMeteringPointRequiredAndInKnownType(string typeOfMeteringPoint, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                ProcessType = BusinessProcessType.CreateMeteringPoint.Name,
                GsrnNumber = SampleData.GsrnNumber,
                TypeOfMeteringPoint = typeOfMeteringPoint,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.Consumption), "", typeof(MeterReadingOccurenceMandatoryValidationError), true)]
        [InlineData(nameof(MeteringPointType.VEProduction), nameof(ReadingOccurrence.Yearly), typeof(MeterReadingOccurenceInvalidValueValidationError), true)]
        [InlineData(nameof(MeteringPointType.VEProduction), nameof(ReadingOccurrence.Monthly), typeof(MeterReadingOccurenceInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.Analysis), nameof(ReadingOccurrence.Monthly), typeof(MeterReadingOccurenceInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.NetConsumption), nameof(ReadingOccurrence.Monthly), typeof(MeterReadingOccurenceInvalidValueValidationError), true)]
        [InlineData(nameof(MeteringPointType.NetConsumption), nameof(ReadingOccurrence.Quarterly), typeof(MeterReadingOccurenceInvalidValueValidationError), true)]
        [InlineData(nameof(MeteringPointType.NetConsumption), nameof(ReadingOccurrence.Hourly), typeof(MeterReadingOccurenceInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup), nameof(ReadingOccurrence.Monthly), typeof(MeterReadingOccurenceInvalidValueValidationError), true)]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup), nameof(ReadingOccurrence.Quarterly), typeof(MeterReadingOccurenceInvalidValueValidationError), true)]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup), nameof(ReadingOccurrence.Hourly), typeof(MeterReadingOccurenceInvalidValueValidationError), false)]
        public void Validate_MeterReadingOccurence(string meteringPointType, string meterReadingOccurence, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                ProcessType = BusinessProcessType.CreateMeteringPoint.Name,
                MeterReadingOccurrence = meterReadingOccurence,
                TypeOfMeteringPoint = meteringPointType,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.Consumption), "gridArea", typeof(SourceMeteringGridAreaNotAllowedValidationError), false)]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy), "gridArea", typeof(SourceMeteringGridAreaNotAllowedValidationError), true)]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy), "", typeof(SourceMeteringGridAreaNotAllowedValidationError), false)]
        public void Validate_SourceMeteringGridArea(string meteringPointType, string gridArea,  System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                ProcessType = BusinessProcessType.CreateMeteringPoint.Name,
                GsrnNumber = SampleData.GsrnNumber,
                TypeOfMeteringPoint = meteringPointType,
                FromGrid = gridArea,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.Consumption), "gridArea", typeof(TargetMeteringGridAreaNotAllowedValidationError), false)]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy), "gridArea", typeof(TargetMeteringGridAreaNotAllowedValidationError), true)]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy), "", typeof(TargetMeteringGridAreaNotAllowedValidationError), false)]

        public void Validate_TargetMeteringGridArea(string meteringPointType, string gridArea,  System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                ProcessType = BusinessProcessType.CreateMeteringPoint.Name,
                GsrnNumber = SampleData.GsrnNumber,
                TypeOfMeteringPoint = meteringPointType,
                ToGrid = gridArea,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        private static MasterDataDocument CreateRequest()
        {
            return new();
        }

        private static List<ValidationError> GetValidationErrors(MasterDataDocument request)
        {
            var ruleSet = new ValidationRuleSet();
            var validationResult = ruleSet.Validate(request);

            return validationResult.Errors
                .Select(error => (ValidationError)error.CustomState)
                .ToList();
        }

        private static void ValidateCreateMeteringPoint(MasterDataDocument businessRequest, System.Type validationError, bool expectedError)
        {
            var errors = GetValidationErrors(businessRequest);
            var errorType = errors.Find(error => error.GetType() == validationError);

            if (expectedError)
            {
                errorType.Should().BeOfType(validationError);
            }
            else
            {
                errorType.Should().BeNull();
            }
        }
    }
}
