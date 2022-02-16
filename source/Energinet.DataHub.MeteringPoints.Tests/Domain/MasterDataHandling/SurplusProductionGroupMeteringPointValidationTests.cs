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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.SurplusProduction;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class SurplusProductionGroupMeteringPointValidationTests : TestBase
    {
        [Fact]
        public void Metering_method_must_remain_calculated()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.SurplusProductionGroup, MasterDataBuilderForConsumption()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithMeteringConfiguration(MeteringMethod.Calculated.Name, null)
                .WithScheduledMeterReadingDate("0101"));

            var updatedMasterData = Updater(meteringPoint)
                .WithMeteringConfiguration(MeteringMethod.Physical.Name, "123456")
                .Build();

            AssertError<MeteringMethodMustRemainCalculatedRuleError>(meteringPoint.CanUpdateMasterData(updatedMasterData, CreateValidator()));
        }

        [Fact]
        public void Production_obligation_is_ignored()
        {
            var masterData = Builder()
                .WithProductionObligation(true)
                .Build();

            Assert.Null(masterData.ProductionObligation);
        }

        [Fact]
        public void Unit_type_must_be_kwh()
        {
            var masterData = Builder()
                .WithMeasurementUnitType(MeasurementUnitType.Ampere.Name)
                .Build();

            AssertError<UnitTypeIsNotValidForMeteringPointType>(CheckRules(masterData), true);
        }

        [Fact]
        public void Unit_type_valid()
        {
            var masterData = Builder()
                .WithMeasurementUnitType(MeasurementUnitType.KWh.Name)
                .Build();

            AssertDoesNotContainValidationError<UnitTypeIsNotValidForMeteringPointType>(CheckRules(masterData));
        }

        [Fact]
        public void Product_type_must_be_energy_active()
        {
            var masterData = Builder()
                .WithProductType(ProductType.Tariff.Name)
                .Build();

            AssertError<InvalidProductType>(CheckRules(masterData), true);
        }

        [Fact]
        public void Power_plant_should_not_be_required()
        {
            var masterData = Builder()
                .WithPowerPlant(null!)
                .Build();

            AssertDoesNotContainValidationError<PowerPlantIsRequired>("D57", CheckRules(masterData));
        }

        [Fact]
        public void Street_name_is_required()
        {
            var masterData = Builder()
                .WithAddress(streetName: string.Empty)
                .Build();

            AssertContainsValidationError<StreetNameIsRequiredRuleError>("E86", CheckRules(masterData));
        }

        [Theory]
        [InlineData(nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(ReadingOccurrence.Yearly), true)]
        public void Meter_reading_periodicity_is_hourly_or_quaterly(string readingOccurrence, bool expectError)
        {
            var masterData = Builder()
                .WithReadingPeriodicity(readingOccurrence)
                .Build();

            AssertError<InvalidMeterReadingOccurrenceRuleError>("D53", CheckRules(masterData), expectError);
        }

        private static IMasterDataBuilder Builder() =>
            new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(MeteringPointType.SurplusProductionGroup))
                .WithReadingPeriodicity(ReadingOccurrence.Quarterly.Name)
                .WithAddress(streetName: "Test street", countryCode: CountryCode.DK);

        private static MasterDataUpdater Updater(MeteringPoint meteringPoint) => new MasterDataUpdater(
            new MasterDataFieldSelector().GetMasterDataFieldsFor(MeteringPointType.SurplusProductionGroup),
            meteringPoint.MasterData);

        private static BusinessRulesValidationResult CheckRules(MasterData masterData)
        {
            return new MasterDataValidator(
                    new SurplusProductionGroupValidator())
                .CheckRulesFor(MeteringPointType.SurplusProductionGroup, masterData);
        }

        private static MasterDataValidator CreateValidator() => new MasterDataValidator(
            new SurplusProductionGroupValidator());
    }
}
