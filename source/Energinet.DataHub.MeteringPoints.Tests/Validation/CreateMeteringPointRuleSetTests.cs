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
        [InlineData("", typeof(OccurenceRequiredValidationError), true)]
        [InlineData("2021-11-12T12:12:12Z", typeof(OccurenceDateWrongFormatValidationError), false)]
        [InlineData("12-12-2021T12:12:12Z", typeof(OccurenceDateWrongFormatValidationError), true)]
        [InlineData("12-12-2021T12:12:12", typeof(OccurenceDateWrongFormatValidationError), true)]
        [InlineData("YYYY-12-12T12:12:12Z", typeof(OccurenceDateWrongFormatValidationError), true)]
        [InlineData("2021-12-12T12:12:12.12Z", typeof(OccurenceDateWrongFormatValidationError), false)]
        [InlineData("2021-12-12T12:12:12.123Z", typeof(OccurenceDateWrongFormatValidationError), false)]
        [InlineData("2021-12-12T12:12:12:1234Z", typeof(OccurenceDateWrongFormatValidationError), true)]
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

        [Theory]
        [InlineData("Tester 1", "Consumption", typeof(StreetNameMandatoryForMeteringPointTypeValidationError), false)]
        [InlineData("Tester 1", "Production", typeof(StreetNameMandatoryForMeteringPointTypeValidationError), false)]
        [InlineData("", "Production", typeof(StreetNameMandatoryForMeteringPointTypeValidationError), true)]
        [InlineData("", "OtherType", typeof(StreetNameMandatoryForMeteringPointTypeValidationError), false)]
        [InlineData("VeryLongAddressNameThatWillReturnValidationErrorForMaxLength", "Consumption", typeof(StreetNameMaximumLengthValidationError), true)]
        [InlineData("VeryLongAddressNameThatWillReturnValidationErrorForMaxLength", "OtherType", typeof(StreetNameMaximumLengthValidationError), true)]
        [InlineData("Tester 1", "OtherType", typeof(StreetNameMaximumLengthValidationError), false)]
        public void Validate_StreetNameRequiredAndMaximumLength(string streetName, string typeOfMeteringPoint, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                StreetName = streetName,
                PostCode = SampleData.PostCode,
                CityName = SampleData.CityName,
                GsrnNumber = SampleData.GsrnNumber,
                TypeOfMeteringPoint = typeOfMeteringPoint,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData("8000", "Consumption", "DK", typeof(PostCodeWrongFormatValidationError), false)]
        [InlineData("800", "Consumption", "DK", typeof(PostCodeWrongFormatValidationError), true)]
        [InlineData("", "OtherType", "DK", typeof(PostCodeMandatoryForMeteringPointTypeValidationError), false)]
        [InlineData("", "Consumption", "DK", typeof(PostCodeMandatoryForMeteringPointTypeValidationError), true)]
        [InlineData("LONGPOSTCODE", "Consumption", "SE", typeof(PostCodeMaximumLengthValidationError), true)]
        [InlineData("POSTCODE", "Consumption", "SE", typeof(PostCodeMaximumLengthValidationError), false)]
        public void Validate_PostCodeRequiredAndFormat(string postCode, string typeOfMeteringPoint, string countryCode, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                StreetName = SampleData.StreetName,
                PostCode = postCode,
                CityName = SampleData.CityName,
                CountryCode = countryCode,
                GsrnNumber = SampleData.GsrnNumber,
                TypeOfMeteringPoint = typeOfMeteringPoint,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData("Aarhus C", "Consumption", typeof(CityNameMandatoryForMeteringPointTypeValidationError), false)]
        [InlineData("København", "Production", typeof(CityNameMandatoryForMeteringPointTypeValidationError), false)]
        [InlineData("", "Production", typeof(CityNameMandatoryForMeteringPointTypeValidationError), true)]
        [InlineData("", "OtherType", typeof(CityNameMandatoryForMeteringPointTypeValidationError), false)]
        [InlineData("Azpilicuetagaraycosaroyarenberecolarrea", "Consumption", typeof(CityNameMaximumLengthValidationError), true)]
        [InlineData("Azpilicuetagaraycosaroyarenberecolarrea", "OtherType", typeof(CityNameMaximumLengthValidationError), true)]
        [InlineData("Aarhus C", "OtherType", typeof(CityNameMaximumLengthValidationError), false)]
        public void Validate_CityNameRequiredAndMaximumLength(string cityName, string typeOfMeteringPoint, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                StreetName = SampleData.StreetName,
                PostCode = SampleData.PostCode,
                CityName = cityName,
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
        [InlineData("1234", typeof(StreetCodeValidationError), false)]
        [InlineData("0000", typeof(StreetCodeValidationError), true)]
        [InlineData("0001", typeof(StreetCodeValidationError), false)]
        [InlineData("00011", typeof(StreetCodeValidationError), true)]
        [InlineData("12345", typeof(StreetCodeValidationError), true)]
        [InlineData("9999", typeof(StreetCodeValidationError), false)]
        [InlineData("9", typeof(StreetCodeValidationError), true)]
        [InlineData("afsd", typeof(StreetCodeValidationError), true)]
        public void Validate_StreetCode(string streetCode, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                StreetCode = streetCode,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData("", typeof(FloorIdentificationValidationError), false)]
        [InlineData("2", typeof(FloorIdentificationValidationError), false)]
        [InlineData("A", typeof(FloorIdentificationValidationError), false)]
        [InlineData("ABCDE", typeof(FloorIdentificationValidationError), true)]
        [InlineData("12345", typeof(FloorIdentificationValidationError), true)]
        public void Validate_FloorIdentification(string floorIdentification, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                FloorIdentification = floorIdentification,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData("", typeof(RoomIdentificationValidationError), false)]
        [InlineData("2", typeof(RoomIdentificationValidationError), false)]
        [InlineData("A", typeof(RoomIdentificationValidationError), false)]
        [InlineData("ABCDE", typeof(RoomIdentificationValidationError), true)]
        [InlineData("12345", typeof(RoomIdentificationValidationError), true)]
        public void Validate_RoomIdentification(string roomIdentification, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                RoomIdentification = roomIdentification,
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
        [InlineData("22A", "DK", typeof(BuildingNumberMustBeValidValidationError), false)]
        [InlineData("AÆZ", "DK", typeof(BuildingNumberMustBeValidValidationError), false)]
        [InlineData("22ADA", "", typeof(BuildingNumberMustBeValidValidationError), false)]
        [InlineData("ÆØÅ", "", typeof(BuildingNumberMustBeValidValidationError), false)]
        [InlineData("1AAA", "", typeof(BuildingNumberMustBeValidValidationError), false)]
        [InlineData("", "DK", typeof(BuildingNumberMustBeValidValidationError), true)]
        [InlineData("001K", "DK", typeof(BuildingNumberMustBeValidValidationError), true)]
        [InlineData("001KA", "DK", typeof(BuildingNumberMustBeValidValidationError), true)]
        [InlineData("2AaIOAK", "", typeof(BuildingNumberMustBeValidValidationError), true)]
        public void Validate_BuildingNumber_Format(string buildingNumber, string countryCode, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                CountryCode = countryCode,
                BuildingNumber = buildingNumber,
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
        [InlineData("", typeof(CitySubDivisionNameMaximumLengthValidationError), false)]
        [InlineData("Asdasdakl k asdasdsa", typeof(CitySubDivisionNameMaximumLengthValidationError), false)]
        [InlineData("Asdkl dasdkjsajkd ksd skladsa lkdasjlk assad sakd sadas asd sa", typeof(CitySubDivisionNameMaximumLengthValidationError), true)]
        public void Validate_CitySubDivisionName(string citySubDivisionName, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                CitySubDivisionName = citySubDivisionName,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData("", typeof(LocationDescriptionMaximumLengthValidationError), false)]
        [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit.", typeof(LocationDescriptionMaximumLengthValidationError), false)]
        [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec semper neque ac lectus accumsan, vel gravida nulla pretium. Ut vitae ipsum leo. Vestibulum consectetur iaculis est venenatis tincidunt. Fusce eu tincidunt ex.", typeof(LocationDescriptionMaximumLengthValidationError), true)]
        public void Validate_LocationDescription(string localDescription, System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                LocationDescription = localDescription,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData("571234567891234568", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.One), typeof(PowerPlantGsrnEan18ValidValidationError), false)]
        [InlineData("571234567891234568", nameof(MeteringPointType.VEProduction), nameof(NetSettlementGroup.One), typeof(PowerPlantGsrnEan18ValidValidationError), false)]
        [InlineData("561234567891234568", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.One), typeof(PowerPlantGsrnEan18ValidValidationError), true)]
        [InlineData("571234567891234568", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One), typeof(PowerPlantGsrnEan18ValidValidationError), false)]
        [InlineData("8891928731", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One), typeof(PowerPlantGsrnEan18ValidValidationError), true)]
        [InlineData("", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError), true)]
        [InlineData("", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError), true)]
        [InlineData("", nameof(MeteringPointType.VEProduction), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError), true)]
        [InlineData("", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.Zero), typeof(PowerPlantGsrnEan18ValidValidationError), false)]
        public void Validate_PowerPlant(string powerPlant, string meteringPointType, string netSettlementGroup,  System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                PowerPlant = powerPlant,
                TypeOfMeteringPoint = meteringPointType,
                NetSettlementGroup = netSettlementGroup,
            };

            ValidateCreateMeteringPoint(businessRequest, validationError, expectedError);
        }

        [Theory]
        [InlineData("571234567891234568", nameof(MeteringPointType.Analysis), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError), true)]
        [InlineData("571234567891234568", nameof(MeteringPointType.Exchange), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError), true)]
        [InlineData("571234567891234568", nameof(MeteringPointType.ExchangeReactiveEnergy), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError), true)]
        [InlineData("571234567891234568", nameof(MeteringPointType.InternalUse), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError), true)]
        [InlineData("571234567891234568", nameof(MeteringPointType.NetConsumption), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError), true)]
        public void Validate_PowerPlant_Not_Allowed(string powerPlant, string meteringPointType, string netSettlementGroup,  System.Type validationError, bool expectedError)
        {
            var businessRequest = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                PowerPlant = powerPlant,
                TypeOfMeteringPoint = meteringPointType,
                NetSettlementGroup = netSettlementGroup,
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
