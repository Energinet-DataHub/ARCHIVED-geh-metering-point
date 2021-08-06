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
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Validation
{
    [UnitTest]
    public class AssetTypeRuleTests : RuleSetTest<CreateMeteringPoint, AssetTypeRule>
    {
        [Theory]
        [InlineData(nameof(AssetType.CombustionEngineGas), nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One))]
        [InlineData(nameof(AssetType.CombustionEngineGas), nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.Six))]
        [InlineData("", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.Zero))]
        [InlineData(null, nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.Zero))]
        public void ShouldValidate(string assetType, string meteringPointType, string netSettlementGroup)
        {
            var request = CreateRequest() with
            {
                NetSettlementGroup = netSettlementGroup,
                AssetType = assetType,
                TypeOfMeteringPoint = meteringPointType,
            };

            var errors = Validate(request);

            errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One), typeof(AssetTypeMandatoryValidationError))]
        [InlineData(null, nameof(MeteringPointType.Production), nameof(NetSettlementGroup.Six), typeof(AssetTypeMandatoryValidationError))]
        [InlineData("", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.Six), typeof(AssetTypeMandatoryValidationError))]
        [InlineData(null, nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.Six), typeof(AssetTypeMandatoryValidationError))]

        [InlineData(nameof(AssetType.Boiler), nameof(MeteringPointType.Analysis), nameof(NetSettlementGroup.One), typeof(AssetTypeNotAllowedValidationError))]
        [InlineData(nameof(AssetType.Boiler), nameof(MeteringPointType.ExchangeReactiveEnergy), nameof(NetSettlementGroup.One), typeof(AssetTypeNotAllowedValidationError))]
        [InlineData(nameof(AssetType.Boiler), nameof(MeteringPointType.InternalUse), nameof(NetSettlementGroup.One), typeof(AssetTypeNotAllowedValidationError))]
        [InlineData(nameof(AssetType.Boiler), nameof(MeteringPointType.Exchange), nameof(NetSettlementGroup.One), typeof(AssetTypeNotAllowedValidationError))]
        [InlineData(nameof(AssetType.Boiler), nameof(MeteringPointType.GridLossCorrection), nameof(NetSettlementGroup.One), typeof(AssetTypeNotAllowedValidationError))]
        [InlineData(nameof(AssetType.Boiler), nameof(MeteringPointType.ElectricalHeating), nameof(NetSettlementGroup.One), typeof(AssetTypeNotAllowedValidationError))]
        [InlineData(nameof(AssetType.Boiler), nameof(MeteringPointType.NetConsumption), nameof(NetSettlementGroup.One), typeof(AssetTypeNotAllowedValidationError))]

        [InlineData("WrongAssetTypeValue", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One), typeof(AssetTypeWrongValueValidationError))]
        public void ShouldResultInError(string assetType, string meteringPointType, string netSettlementGroup, Type expectedError)
        {
            var request = CreateRequest() with
            {
                NetSettlementGroup = netSettlementGroup,
                AssetType = assetType,
                TypeOfMeteringPoint = meteringPointType,
            };

            var errors = Validate(request);

            errors.Should().ContainSingle(error => error.GetType() == expectedError);
        }

        private static CreateMeteringPoint CreateRequest()
        {
            return new();
        }
    }
}
