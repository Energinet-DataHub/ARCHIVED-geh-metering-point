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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class VEProductionMeteringPointValidationTests : TestBase
    {
        [Fact]
        public void Product_type_is_required()
        {
            var validationResult = Builder()
                .Validate();

            AssertContainsValidationError<ProductTypeIsRequired>(validationResult);
        }

        [Fact]
        public void Power_plant_should_not_be_required()
        {
            var masterData = Builder()
                .WithPowerPlant(null!)
                .Build();

            AssertDoesNotContainValidationError<PowerPlantRequirementRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Street_name_is_required()
        {
            var masterData = Builder()
                .WithAddress(streetName: string.Empty)
                .Build();

            AssertContainsValidationError<StreetNameIsRequiredRuleError>(CheckRules(masterData));
        }

        [Theory]
        [InlineData(nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(ReadingOccurrence.Monthly), false)]
        [InlineData(nameof(ReadingOccurrence.Yearly), true)]
        public void Meter_reading_periodicity_is_hourly_or_quarterly_or_monthly(string readingOccurrence, bool expectError)
        {
            var masterData = Builder()
                .WithReadingPeriodicity(readingOccurrence)
                .Build();

            AssertError<InvalidMeterReadingOccurrenceRuleError>(CheckRules(masterData), expectError);
        }

        private static IMasterDataBuilder Builder() =>
            new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(MeteringPointType.VEProduction))
                .WithAddress(streetName: "Test Street", countryCode: CountryCode.DK)
                .WithReadingPeriodicity(ReadingOccurrence.Yearly.Name);

        private static BusinessRulesValidationResult CheckRules(MasterData masterData)
        {
            return new MasterDataValidator().CheckRulesFor(MeteringPointType.VEProduction, masterData);
        }
    }
}
