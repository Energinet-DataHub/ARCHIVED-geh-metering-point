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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class SpecialMeteringPointValidationTests : TestBase
    {
        [Theory]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup))]
        public void Production_obligation_is_ignored(string meteringPointType)
        {
            var masterData = BuilderFor(meteringPointType)
                .WithProductionObligation(true)
                .Build();

            Assert.Null(masterData.ProductionObligation);
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup))]
        public void Unit_type_must_be_kwh(string meteringPointType)
        {
            var masterData = BuilderFor(meteringPointType)
                .WithMeasurementUnitType(MeasurementUnitType.Ampere.Name)
                .Build();

            AssertError<UnitTypeIsNotValidForMeteringPointType>(CheckRules(masterData, From(meteringPointType)), true);
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup))]
        public void Unit_type_valid(string meteringPointType)
        {
            var masterData = BuilderFor(meteringPointType)
                .WithMeasurementUnitType(MeasurementUnitType.KWh.Name)
                .Build();

            AssertDoesNotContainValidationError<UnitTypeIsNotValidForMeteringPointType>(CheckRules(masterData, From(meteringPointType)));
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup))]
        public void Product_type_must_be_energy_active(string meteringPointType)
        {
            var masterData = BuilderFor(meteringPointType)
                .WithProductType(ProductType.Tariff.Name)
                .Build();

            AssertError<InvalidProductType>(CheckRules(masterData, From(meteringPointType)), true);
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup))]
        [InlineData(nameof(MeteringPointType.VEProduction))]
        public void Power_plant_should_not_be_required(string meteringPointType)
        {
            var masterData = BuilderFor(meteringPointType)
                .WithPowerPlant(null!)
                .Build();

            AssertDoesNotContainValidationError<PowerPlantIsRequired>(CheckRules(masterData, From(meteringPointType)));
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup))]
        [InlineData(nameof(MeteringPointType.VEProduction))]
        public void Street_name_is_required(string meteringPointType)
        {
            var masterData = BuilderFor(meteringPointType)
                .WithAddress(streetName: string.Empty)
                .Build();

            AssertContainsValidationError<StreetNameIsRequiredRuleError>(CheckRules(masterData, From(meteringPointType)));
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup), nameof(ReadingOccurrence.Yearly), true)]
        public void Meter_reading_periodicity_is_hourly_or_quaterly(string meteringPointType, string readingOccurrence, bool expectError)
        {
            var masterData = BuilderFor(meteringPointType)
                .WithReadingPeriodicity(readingOccurrence)
                .Build();

            AssertError<InvalidMeterReadingOccurrenceRuleError>(CheckRules(masterData, From(meteringPointType)), expectError);
        }

        private static IMasterDataBuilder BuilderFor(string meteringPointType) =>
            new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(From(meteringPointType)))
                .WithReadingPeriodicity(ReadingOccurrence.Quarterly.Name)
                .WithAddress(streetName: "Test street", countryCode: CountryCode.DK);

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
