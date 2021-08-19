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
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    [UnitTest]
    public class MeteringPointSubTypeRuleTests : CreateMeteringPointRulesTest<MeteringPointSubTypeMustBeValidRule>
    {
        [Theory]
        [InlineData(nameof(MeteringPointType.Production), nameof(MeteringPointSubType.Virtual), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeteringPointSubType.Calculated), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(MeteringPointType.WholesaleServices), nameof(MeteringPointSubType.Virtual), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(MeteringPointType.OwnProduction), nameof(MeteringPointSubType.Virtual), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(MeteringPointType.NetFromGrid), nameof(MeteringPointSubType.Calculated), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(MeteringPointType.NetToGrid), nameof(MeteringPointSubType.Calculated), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(MeteringPointType.TotalConsumption), nameof(MeteringPointSubType.Calculated), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeteringPointSubType.Physical), nameof(NetSettlementGroup.Zero))]
        [InlineData(nameof(MeteringPointType.Production), nameof(MeteringPointSubType.Physical), nameof(NetSettlementGroup.Ninetynine))]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeteringPointSubType.Virtual), nameof(NetSettlementGroup.Zero))]
        [InlineData(nameof(MeteringPointType.Production), nameof(MeteringPointSubType.Calculated), nameof(NetSettlementGroup.Ninetynine))]
        [InlineData(nameof(MeteringPointType.OtherConsumption), nameof(MeteringPointSubType.Physical), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(MeteringPointType.OtherProduction), nameof(MeteringPointSubType.Virtual), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy), nameof(MeteringPointSubType.Virtual), nameof(NetSettlementGroup.One))]
        public void MeteringPointSubtypeShouldValidate(string typeOfMeteringPoint, string meteringPointSubType, string netSettlementGroup)
        {
            var request = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                TypeOfMeteringPoint = typeOfMeteringPoint,
                SubTypeOfMeteringPoint = meteringPointSubType,
                NetSettlementGroup = netSettlementGroup,
            };

            ShouldValidateWithNoErrors(request);
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.Consumption), "", nameof(NetSettlementGroup.One),   typeof(MeteringPointTypeConsumptionOrProductionIsNotInNetSettlementZeroOrNinetyNineMustBeSubtypeVirtualOrCalculatedValidationError))]
        [InlineData(nameof(MeteringPointType.WholesaleServices), nameof(MeteringPointSubType.Physical), nameof(NetSettlementGroup.One),   typeof(MeteringPointSubTypeValueMustBeValidValidationError))]
        [InlineData(nameof(MeteringPointType.OwnProduction), nameof(MeteringPointSubType.Physical), nameof(NetSettlementGroup.One),   typeof(MeteringPointSubTypeValueMustBeValidValidationError))]
        [InlineData(nameof(MeteringPointType.NetFromGrid), nameof(MeteringPointSubType.Physical), nameof(NetSettlementGroup.One),   typeof(MeteringPointSubTypeValueMustBeValidValidationError))]
        [InlineData(nameof(MeteringPointType.NetToGrid), nameof(MeteringPointSubType.Physical), nameof(NetSettlementGroup.One),   typeof(MeteringPointSubTypeValueMustBeValidValidationError))]
        [InlineData(nameof(MeteringPointType.TotalConsumption), nameof(MeteringPointSubType.Physical), nameof(NetSettlementGroup.One),   typeof(MeteringPointSubTypeValueMustBeValidValidationError))]
        [InlineData(nameof(MeteringPointType.Production), nameof(MeteringPointSubType.Physical), nameof(NetSettlementGroup.One), typeof(MeteringPointTypeConsumptionOrProductionIsNotInNetSettlementZeroOrNinetyNineMustBeSubtypeVirtualOrCalculatedValidationError))]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeteringPointSubType.Physical), nameof(NetSettlementGroup.One), typeof(MeteringPointTypeConsumptionOrProductionIsNotInNetSettlementZeroOrNinetyNineMustBeSubtypeVirtualOrCalculatedValidationError))]
        [InlineData(nameof(MeteringPointType.OtherConsumption), nameof(MeteringPointSubType.Calculated), nameof(NetSettlementGroup.One),   typeof(MeteringPointSubTypeMustBePhysicalOrVirtualValidationError))]
        [InlineData(nameof(MeteringPointType.OtherProduction), nameof(MeteringPointSubType.Calculated), nameof(NetSettlementGroup.One),   typeof(MeteringPointSubTypeMustBePhysicalOrVirtualValidationError))]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy), nameof(MeteringPointSubType.Calculated), nameof(NetSettlementGroup.One),   typeof(MeteringPointSubTypeMustBePhysicalOrVirtualValidationError))]
        public void MeteringPointSubtypeShouldResultInError(string typeOfMeteringPoint, string meteringPointSubType, string netSettlementGroup,  Type expectedError)
        {
            var request = CreateRequest() with
            {
                GsrnNumber = SampleData.GsrnNumber,
                TypeOfMeteringPoint = typeOfMeteringPoint,
                SubTypeOfMeteringPoint = meteringPointSubType,
                NetSettlementGroup = netSettlementGroup,
            };

            ShouldValidateWithSingleError(request, expectedError);
        }
    }
}
