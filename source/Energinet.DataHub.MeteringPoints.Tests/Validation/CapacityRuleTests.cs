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
    public class CapacityRuleTests : CreateMeteringPointRulesTest<CapacityRule>
    {
        [Theory]
        [InlineData("12345678", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One))]
        [InlineData("2234.567", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One))]
        [InlineData("32345678", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.Zero))]
        [InlineData("42345678", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.One))]
        [InlineData("", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.Zero))]
        [InlineData(null, nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.Zero))]
        public void CapacityShouldValidate(string capacity, string meteringPointType, string netSettlementGroup)
        {
            var request = CreateRequest() with
            {
                NetSettlementGroup = netSettlementGroup,
                PhysicalConnectionCapacity = capacity,
                TypeOfMeteringPoint = meteringPointType,
            };

            var errors = Validate(request);

            errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("423456789", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One), typeof(CapacityMaximumLengthValidationError))]
        [InlineData("5234.5678", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One), typeof(CapacityMaximumLengthValidationError))]
        [InlineData("1,2", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One), typeof(CapacityMaximumLengthValidationError))]
        [InlineData("1.2.3", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One), typeof(CapacityMaximumLengthValidationError))]

        [InlineData(null, nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One), typeof(CapacityIsMandatoryValidationError))]
        [InlineData("", nameof(MeteringPointType.Production), nameof(NetSettlementGroup.One), typeof(CapacityIsMandatoryValidationError))]
        [InlineData(null, nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.One), typeof(CapacityIsMandatoryValidationError))]
        [InlineData("", nameof(MeteringPointType.Consumption), nameof(NetSettlementGroup.One), typeof(CapacityIsMandatoryValidationError))]

        [InlineData("", nameof(MeteringPointType.Exchange), nameof(NetSettlementGroup.One), typeof(CapacityIsNotAllowedValidationError))]
        [InlineData("", nameof(MeteringPointType.Analysis), nameof(NetSettlementGroup.One), typeof(CapacityIsNotAllowedValidationError))]
        [InlineData("", nameof(MeteringPointType.NetConsumption), nameof(NetSettlementGroup.One), typeof(CapacityIsNotAllowedValidationError))]
        [InlineData("", nameof(MeteringPointType.ExchangeReactiveEnergy), nameof(NetSettlementGroup.One), typeof(CapacityIsNotAllowedValidationError))]
        [InlineData("", nameof(MeteringPointType.InternalUse), nameof(NetSettlementGroup.One), typeof(CapacityIsNotAllowedValidationError))]
        public void CapacityShouldResultInError(string capacity, string meteringPointType, string netSettlementGroup, Type expectedError)
        {
            var request = CreateRequest() with
            {
                NetSettlementGroup = netSettlementGroup,
                PhysicalConnectionCapacity = capacity,
                TypeOfMeteringPoint = meteringPointType,
            };

            var errors = Validate(request);

            errors.Should().ContainSingle(error => error.GetType() == expectedError);
        }
    }
}
