// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    public class PowerPlantRuleTests : CreateMeteringPointRulesTest<PowerPlantMustBeValidRule>
    {
        [Theory]
        [InlineData("571234567891234568", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.One))]
        [InlineData("571234567891234568", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One))]
        [InlineData("", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.Zero))]
        [InlineData("", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.Ninetynine))]
        public void PowerPlantShouldValidate(string powerPlant, string meteringPointType, string netSettlementGroup)
        {
            var request = CreateRequest() with
            {
                PowerPlant = powerPlant,
                TypeOfMeteringPoint = meteringPointType,
                NetSettlementGroup = netSettlementGroup,
            };

            ShouldValidateWithNoErrors(request);
        }

        [Theory]
        [InlineData("", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError))]
        [InlineData("", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError))]
        [InlineData("8891928731459", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One), typeof(PowerPlantGsrnEan18ValidValidationError))]
        [InlineData("561234567891234568", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.One), typeof(PowerPlantGsrnEan18ValidValidationError))]
        [InlineData("571234567891234568", nameof(MeteringPointType.Analysis), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError))]
        [InlineData("571234567891234568", nameof(MeteringPointType.Exchange), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError))]
        [InlineData("571234567891234568", nameof(MeteringPointType.ExchangeReactiveEnergy), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError))]
        [InlineData("571234567891234568", nameof(MeteringPointType.InternalUse), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError))]
        [InlineData("571234567891234568", nameof(MeteringPointType.NetConsumption), nameof(NetSettlementGroup.One), typeof(PowerPlantValidationError))]
        public void PowerPlantShouldResultInError(string powerPlant, string meteringPointType, string netSettlementGroup, Type expectedError)
        {
            var request = CreateRequest() with
            {
                PowerPlant = powerPlant,
                TypeOfMeteringPoint = meteringPointType,
                NetSettlementGroup = netSettlementGroup,
            };

            ShouldValidateWithSingleError(request, expectedError);
        }
    }
}
