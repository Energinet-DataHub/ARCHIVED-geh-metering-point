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

using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class OtherProductionMeteringPointValidationTests : TestBase
    {
        [Fact]
        public void Production_obligation_is_ignored()
        {
            var masterData = BuilderFor(MeteringPointType.OtherProduction.Name, "Virtual")
                .WithProductionObligation(true)
                .Build();

            Assert.Null(masterData.ProductionObligation);
        }

        [Fact]
        public void Unit_type_must_be_kwh()
        {
            var masterData = BuilderFor(MeteringPointType.OtherProduction.Name, "Virtual")
                .WithMeasurementUnitType(MeasurementUnitType.Ampere.Name)
                .Build();

            AssertError<UnitTypeIsNotValidForMeteringPointType>(CheckRules(masterData, From(MeteringPointType.OtherProduction.Name)), true);
        }

        [Fact]
        public void Metering_method_must_be_physical_or_virtual()
        {
            var masterData = BuilderFor(MeteringPointType.OtherProduction.Name, "Calculated")
                .Build();

            AssertError<MeteringMethodMustBePhysicalOrVirtualRuleError>(CheckRules(masterData, From(MeteringPointType.OtherProduction.Name)), true);
        }

        [Fact]
        public void Unit_type_valid()
        {
            var masterData = BuilderFor(MeteringPointType.OtherProduction.Name, "Virtual")
                .WithMeasurementUnitType(MeasurementUnitType.KWh.Name)
                .Build();

            AssertDoesNotContainValidationError<UnitTypeIsNotValidForMeteringPointType>(CheckRules(masterData, From(MeteringPointType.OtherProduction.Name)));
        }

        [Fact]
        public void Product_type_must_be_energy_active()
        {
            var masterData = BuilderFor(MeteringPointType.OtherProduction.Name, "Virtual")
                .WithProductType(ProductType.Tariff.Name)
                .Build();

            AssertError<InvalidProductType>(CheckRules(masterData, From(MeteringPointType.OtherProduction.Name)), true);
        }

        [Fact]
        public void Power_plant_should_not_be_required()
        {
            var masterData = BuilderFor(MeteringPointType.OtherProduction.Name, "Virtual")
                .WithPowerPlant(null!)
                .Build();

            AssertDoesNotContainValidationError<PowerPlantIsRequired>(CheckRules(masterData, From(MeteringPointType.OtherProduction.Name)));
        }

        [Fact]
        public void Street_name_is_required()
        {
            var masterData = BuilderFor(MeteringPointType.OtherProduction.Name, "Virtual")
                .WithAddress(streetName: string.Empty)
                .Build();

            AssertContainsValidationError<StreetNameIsRequiredRuleError>(CheckRules(masterData, From(MeteringPointType.OtherProduction.Name)));
        }

        [Theory]
        [InlineData(nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(ReadingOccurrence.Yearly), true)]
        public void Meter_reading_periodicity_is_hourly_or_quaterly(string readingOccurrence, bool expectError)
        {
            var masterData = BuilderFor(MeteringPointType.OtherProduction.Name, "Virtual")
                .WithReadingPeriodicity(readingOccurrence)
                .Build();

            AssertError<InvalidMeterReadingOccurrenceRuleError>(CheckRules(masterData, From(MeteringPointType.OtherProduction.Name)), expectError);
        }

        private static IMasterDataBuilder BuilderFor(string meteringPointType, string meteringConfiguration) =>
            new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(From(meteringPointType)))
                .WithReadingPeriodicity(ReadingOccurrence.Quarterly.Name)
                .WithAddress(streetName: "Test street", countryCode: CountryCode.DK)
                .WithMeteringConfiguration(meteringConfiguration, null);

        private static BusinessRulesValidationResult CheckRules(MasterData masterData, MeteringPointType meteringPointType)
        {
            return new MasterDataValidator().CheckRulesFor(meteringPointType, masterData);
        }

        private static MeteringPointType From(string meteringPointType)
        {
            return EnumerationType.FromName<MeteringPointType>(meteringPointType);
        }
    }
}
