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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production.Rules;
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
        [InlineData(nameof(MeteringPointType.Analysis))]
        [InlineData(nameof(MeteringPointType.ElectricalHeating))]
        [InlineData(nameof(MeteringPointType.InternalUse))]
        [InlineData(nameof(MeteringPointType.NetConsumption))]
        [InlineData(nameof(MeteringPointType.NetProduction))]
        [InlineData(nameof(MeteringPointType.OtherConsumption))]
        [InlineData(nameof(MeteringPointType.OtherProduction))]
        [InlineData(nameof(MeteringPointType.OwnProduction))]
        [InlineData(nameof(MeteringPointType.TotalConsumption))]
        [InlineData(nameof(MeteringPointType.WholesaleServices))]
        [InlineData(nameof(MeteringPointType.ConsumptionFromGrid))]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy))]
        [InlineData(nameof(MeteringPointType.GridLossCorrection))]
        [InlineData(nameof(MeteringPointType.NetFromGrid))]
        [InlineData(nameof(MeteringPointType.NetToGrid))]
        [InlineData(nameof(MeteringPointType.SupplyToGrid))]
        [InlineData(nameof(MeteringPointType.SurplusProductionGroup))]
        [InlineData(nameof(MeteringPointType.VEProduction))]
        public void Power_plant_should_not_be_required(string meteringPointType)
        {
            var masterData = BuilderFor(meteringPointType)
                .WithPowerPlant(null!)
                .Build();

            AssertDoesNotContainValidationError<PowerPlantRequirementRuleError>(CheckRules(masterData, From(meteringPointType)));
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.Analysis))]
        [InlineData(nameof(MeteringPointType.ElectricalHeating))]
        [InlineData(nameof(MeteringPointType.InternalUse))]
        [InlineData(nameof(MeteringPointType.NetConsumption))]
        [InlineData(nameof(MeteringPointType.NetProduction))]
        [InlineData(nameof(MeteringPointType.OtherConsumption))]
        [InlineData(nameof(MeteringPointType.OtherProduction))]
        [InlineData(nameof(MeteringPointType.OwnProduction))]
        [InlineData(nameof(MeteringPointType.TotalConsumption))]
        [InlineData(nameof(MeteringPointType.WholesaleServices))]
        [InlineData(nameof(MeteringPointType.ConsumptionFromGrid))]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy))]
        [InlineData(nameof(MeteringPointType.GridLossCorrection))]
        [InlineData(nameof(MeteringPointType.NetFromGrid))]
        [InlineData(nameof(MeteringPointType.NetToGrid))]
        [InlineData(nameof(MeteringPointType.SupplyToGrid))]
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
        [InlineData(nameof(MeteringPointType.Analysis), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.Analysis), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.Analysis), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.ElectricalHeating), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.ElectricalHeating), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.ElectricalHeating), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.InternalUse), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.InternalUse), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.InternalUse), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.NetConsumption), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.NetConsumption), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.NetConsumption), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.NetProduction), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.NetProduction), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.NetProduction), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.OtherConsumption), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.OtherConsumption), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.OtherConsumption), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.OtherProduction), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.OtherProduction), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.OtherProduction), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.OwnProduction), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.OwnProduction), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.OwnProduction), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.TotalConsumption), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.TotalConsumption), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.TotalConsumption), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.WholesaleServices), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.WholesaleServices), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.WholesaleServices), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.ConsumptionFromGrid), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.ConsumptionFromGrid), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.ConsumptionFromGrid), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.GridLossCorrection), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.GridLossCorrection), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.GridLossCorrection), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.NetFromGrid), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.NetFromGrid), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.NetFromGrid), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.NetToGrid), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.NetToGrid), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.NetToGrid), nameof(ReadingOccurrence.Yearly), true)]

        [InlineData(nameof(MeteringPointType.SupplyToGrid), nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(MeteringPointType.SupplyToGrid), nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(MeteringPointType.SupplyToGrid), nameof(ReadingOccurrence.Yearly), true)]

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
