﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Exchange;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class ExchangeMeteringPointValidationTests : TestBase
    {
        [Fact]
        public void Production_obligation_is_ignored()
        {
            var masterData = Builder()
                .WithProductionObligation(true)
                .Build();

            Assert.Null(masterData.ProductionObligation);
        }

        [Theory]
        [InlineData(nameof(MeasurementUnitType.KWh), false)]
        [InlineData(nameof(MeasurementUnitType.Ampere), true)]
        public void Unit_type_must_be_kwh(string measurementUnitType, bool expectError)
        {
            var masterData = Builder()
                .WithMeasurementUnitType(measurementUnitType)
                .Build();

            AssertError<UnitTypeIsNotValidForMeteringPointType>(CheckRules(masterData), expectError);
        }

        [Fact]
        public void Street_name_is_required()
        {
            var masterData = Builder()
                .WithAddress(streetName: string.Empty)
                .Build();

            AssertContainsValidationError<StreetNameIsRequiredRuleError>("E86", CheckRules(masterData));
        }

        [Fact]
        public void Geo_info_reference_is_required()
        {
            var masterData = Builder()
                .WithAddress(geoInfoReference: null)
                .Build();

            AssertContainsValidationError<GeoInfoReferenceIsRequiredRuleError>("E86", CheckRules(masterData));
        }

        [Fact]
        public void Meter_reading_occurrence_must_be_quarterly_or_hourly()
        {
            var masterData = Builder()
                .WithReadingPeriodicity(ReadingOccurrence.Yearly.Name)
                .Build();

            AssertContainsValidationError<InvalidMeterReadingOccurrenceRuleError>("D53", CheckRules(masterData));
        }

        [Theory]
        [InlineData(nameof(ProductType.EnergyActive), false)]
        [InlineData(nameof(ProductType.FuelQuantity), true)]
        public void Product_type_must_be_correct(string productType, bool expectError)
        {
            var masterData = Builder()
                .WithProductType(productType)
                .Build();

            AssertError<InvalidProductType>("E29", CheckRules(masterData), expectError);
        }

        private static IMasterDataBuilder Builder() =>
            new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(MeteringPointType.Exchange))
                .WithReadingPeriodicity(ReadingOccurrence.Quarterly.Name)
                .WithAddress(streetName: "Test Street", countryCode: CountryCode.DK);

        private static BusinessRulesValidationResult CheckRules(MasterData masterData)
        {
            return new MasterDataValidator(
                new ExchangeMeteringPointValidator())
                .CheckRulesFor(MeteringPointType.Exchange, masterData);
        }
    }
}
