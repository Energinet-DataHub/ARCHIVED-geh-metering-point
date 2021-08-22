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
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.Validation;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentAssertions;
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
        [InlineData("GridLossCorrection", "Flex", typeof(SettlementMethodRequiredValidationError), false)]
        [InlineData("Consumption", "", typeof(SettlementMethodRequiredValidationError), true)]
        [InlineData("GridLossCorrection", "", typeof(SettlementMethodRequiredValidationError), true)]
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
        [InlineData("", typeof(EffectiveDateRequiredValidationError), true)]
        [InlineData("2021-11-12T12:12:12Z", typeof(EffectiveDateWrongFormatValidationError), false)]
        [InlineData("12-12-2021T12:12:12Z", typeof(EffectiveDateWrongFormatValidationError), true)]
        [InlineData("12-12-2021T12:12:12", typeof(EffectiveDateWrongFormatValidationError), true)]
        [InlineData("YYYY-12-12T12:12:12Z", typeof(EffectiveDateWrongFormatValidationError), true)]
        [InlineData("2021-12-12T12:12:12.12Z", typeof(EffectiveDateWrongFormatValidationError), false)]
        [InlineData("2021-12-12T12:12:12.123Z", typeof(EffectiveDateWrongFormatValidationError), false)]
        [InlineData("2021-12-12T12:12:12:1234Z", typeof(EffectiveDateWrongFormatValidationError), true)]
        public void Validate_OccurenceDateMandatoryAndFormat(string occurenceDate, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                EffectiveDate = occurenceDate,
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

        [Theory]
        [InlineData("1234", "Physical", typeof(MeterNumberMandatoryValidationError), false)]
        [InlineData("", "Physical", typeof(MeterNumberMandatoryValidationError), true)]
        [InlineData("1234", "OtherSubType", typeof(MeterNumberNotAllowedValidationError), true)]
        [InlineData("", "OtherSubType", typeof(MeterNumberNotAllowedValidationError), false)]
        public void Validate_MeterNumberMandatoryForPhysicalMP_MeterNumberNotAllowedForOtherMPTypes(string meterNumber, string subTypeOfMeteringPoint, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                MeterNumber = meterNumber,
                SubTypeOfMeteringPoint = subTypeOfMeteringPoint,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData("1234", "Physical", typeof(MeterNumberMaximumLengthValidationError), false)]
        [InlineData("1234567890000000", "Physical", typeof(MeterNumberMaximumLengthValidationError), true)]
        public void Validate_MeterNumberMaximumLength(string meterNumber, string subTypeOfMeteringPoint, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                MeterNumber = meterNumber,
                SubTypeOfMeteringPoint = subTypeOfMeteringPoint,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData("Physical", typeof(MeteringPointSubTypeMandatoryValidationError), false)]
        [InlineData("", typeof(MeteringPointSubTypeMandatoryValidationError), true)]
        public void Validate_MandatorySubTypeOfMP(string subTypeOfMeteringPoint, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                SubTypeOfMeteringPoint = subTypeOfMeteringPoint,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.Ninetynine), typeof(NetSettlementGroupMandatoryValidationError), false)]
        [InlineData(nameof(MeteringPointType.Production), nameof(NetSettlementGroup.Ninetynine), typeof(NetSettlementGroupMandatoryValidationError), false)]
        [InlineData(nameof(MeteringPointType.Production), "InvalidNetSettlementGroupValue", typeof(NetSettlementGroupInvalidValueValidationError), true)]
        [InlineData(nameof(MeteringPointType.Exchange), nameof(NetSettlementGroup.Ninetynine), typeof(NetSettlementGroupNotAllowedValidationError), true)]
        [InlineData(nameof(MeteringPointType.Consumption), "", typeof(NetSettlementGroupMandatoryValidationError), true)]
        [InlineData(nameof(MeteringPointType.Production), "", typeof(NetSettlementGroupMandatoryValidationError), true)]
        [InlineData(nameof(MeteringPointType.Exchange), "", typeof(NetSettlementGroupMandatoryValidationError), false)]
        public void Validate_NetSettlementGroup(string meteringPointType, string netSettlementGroup, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                NetSettlementGroup = netSettlementGroup,
                TypeOfMeteringPoint = meteringPointType,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(ProductType.EnergyActive), typeof(ProductTypeMandatoryValidationError), false)]
        [InlineData(nameof(MeteringPointType.Consumption), "", typeof(ProductTypeMandatoryValidationError), true)]
        [InlineData(nameof(MeteringPointType.Consumption), "InvalidProductType", typeof(ProductTypeInvalidValueValidationError), true)]
        [InlineData(nameof(MeteringPointType.Analysis), nameof(ProductType.PowerReactive), typeof(ProductTypeWrongDefaultValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(ProductType.PowerReactive), typeof(ProductTypeWrongDefaultValueValidationError), true)]
        public void Validate_ProductType(string meteringPointType, string productType, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                ProductType = productType,
                TypeOfMeteringPoint = meteringPointType,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeasurementUnitType.KWh), typeof(MeasureUnitTypeMandatoryValidationError), false)]
        [InlineData(nameof(MeteringPointType.Consumption), "", typeof(MeasureUnitTypeMandatoryValidationError), true)]
        [InlineData(nameof(MeteringPointType.Consumption), "InvalidMeasureUnitType", typeof(MeasureUnitTypeInvalidValueValidationError), true)]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy), nameof(MeasurementUnitType.KVArh), typeof(MeasureUnitTypeInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy), nameof(MeasurementUnitType.KW), typeof(MeasureUnitTypeInvalidValueValidationError), true)]
        [InlineData(nameof(MeteringPointType.OtherConsumption), nameof(MeasurementUnitType.MWh), typeof(MeasureUnitTypeInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.OtherConsumption), nameof(MeasurementUnitType.KWh), typeof(MeasureUnitTypeInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.OtherConsumption), nameof(MeasurementUnitType.MVAr), typeof(MeasureUnitTypeInvalidValueValidationError), true)]
        [InlineData(nameof(MeteringPointType.OtherProduction), nameof(MeasurementUnitType.MWh), typeof(MeasureUnitTypeInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.OtherProduction), nameof(MeasurementUnitType.KWh), typeof(MeasureUnitTypeInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.OtherProduction), nameof(MeasurementUnitType.MVAr), typeof(MeasureUnitTypeInvalidValueValidationError), true)]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeasurementUnitType.KWh), typeof(MeasureUnitTypeInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeasurementUnitType.KVArh), typeof(MeasureUnitTypeInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeasurementUnitType.KW), typeof(MeasureUnitTypeInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeasurementUnitType.MW), typeof(MeasureUnitTypeInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeasurementUnitType.MWh), typeof(MeasureUnitTypeInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeasurementUnitType.Tonne), typeof(MeasureUnitTypeInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeasurementUnitType.MVAr), typeof(MeasureUnitTypeInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeasurementUnitType.DanishTariffCode), typeof(MeasureUnitTypeInvalidValueValidationError), false)]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeasurementUnitType.Ampere), typeof(MeasureUnitTypeInvalidValueValidationError), true)]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeasurementUnitType.STK), typeof(MeasureUnitTypeInvalidValueValidationError), true)]
        public void Validate_MeasureUnitType(string meteringPointType, string measureUnitType, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                MeasureUnitType = measureUnitType,
                TypeOfMeteringPoint = meteringPointType,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(ReadingOccurrence.Hourly), typeof(MeterReadingOccurenceMandatoryValidationError), false)]
        [InlineData(nameof(MeteringPointType.Consumption), "", typeof(MeterReadingOccurenceMandatoryValidationError), true)]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(ReadingOccurrence.Yearly), typeof(MeterReadingOccurenceInvalidValueValidationError), true)]
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
                MeterReadingOccurrence = meterReadingOccurence,
                TypeOfMeteringPoint = meteringPointType,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData("123", typeof(MunicipalityCodeMustBeValidValidationError), false)]
        [InlineData("999", typeof(MunicipalityCodeMustBeValidValidationError), false)]
        [InlineData("1234", typeof(MunicipalityCodeMustBeValidValidationError), true)]
        [InlineData("12", typeof(MunicipalityCodeMustBeValidValidationError), true)]
        public void Validate_Municipality_Code(string municipalityCode, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                MunicipalityCode = municipalityCode,
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
                GsrnNumber = SampleData.GsrnNumber,
                TypeOfMeteringPoint = meteringPointType,
                ToGrid = gridArea,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        private static CreateMeteringPoint CreateRequest()
        {
            return new();
        }

        private static List<ValidationError> GetValidationErrors(CreateMeteringPoint request)
        {
            var ruleSet = new CreateMeteringPointRuleSet();
            var validationResult = ruleSet.Validate(request);

            return validationResult.Errors
                .Select(error => (ValidationError)error.CustomState)
                .ToList();
        }

        private static void ValidateCreateMeteringPoint(CreateMeteringPoint businessRequest, System.Type validationError, bool expectedError)
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
