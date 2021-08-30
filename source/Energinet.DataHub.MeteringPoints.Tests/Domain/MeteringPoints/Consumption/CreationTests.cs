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
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.Consumption
{
    [UnitTest]
    public class CreationTests
    {
        [Theory]
        [InlineData(nameof(NetSettlementGroup.Six))]
        public void Powerplant_GSRN_is_required_when_netsettlementgroup_is_other_than_0_or_99(string netSettlementGroupName)
        {
            var netSettlementGroup = NetSettlementGroup.FromName<NetSettlementGroup>(netSettlementGroupName);
            var checkResult = CreateRequest(netSettlementGroup);

            AssertContainsValidationError<PowerPlantIsRequiredForNetSettlementGroupRuleError>(checkResult);
        }

        [Theory]
        [InlineData(nameof(NetSettlementGroup.Zero))]
        [InlineData(nameof(NetSettlementGroup.Ninetynine))]
        public void Powerplant_GSRN_is_not_required_when_netsettlementgroup_is_0_or_99(string netSettlementGroupName)
        {
            var netSettlementGroup = NetSettlementGroup.FromName<NetSettlementGroup>(netSettlementGroupName);
            var checkResult = CreateRequest(netSettlementGroup);

            AssertDoesNotContainValidationError<PowerPlantIsRequiredForNetSettlementGroupRuleError>(checkResult);
        }

        [Fact]
        public void Street_name_is_required()
        {
            var checkResult = CreateRequest(NetSettlementGroup.One);
            AssertContainsValidationError<StreetNameIsRequiredRuleError>(checkResult);
            Assert.Contains(checkResult.Errors, error => error is StreetNameIsRequiredRuleError);
        }

        [Fact]
        public void Should_return_error_when_post_code_is_missing()
        {
            var address = Address.Create(
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: string.Empty,
                city: string.Empty,
                citySubDivision: string.Empty,
                postCode: string.Empty,
                countryCode: string.Empty,
                floor: string.Empty,
                room: string.Empty,
                municipalityCode: default);

            var checkResult = ConsumptionMeteringPoint.CanCreate(
                meteringPointGSRN: GsrnNumber.Create(SampleData.GsrnNumber),
                NetSettlementGroup.One,
                null,
                address);

            Assert.Contains(checkResult.Errors, error => error is PostCodeIsRequiredRuleError);
        }

        private static BusinessRulesValidationResult CreateRequest(NetSettlementGroup netSettlementGroup)
        {
            return ConsumptionMeteringPoint.CanCreate(
                meteringPointGSRN: GsrnNumber.Create(SampleData.GsrnNumber),
                netSettlementGroup: netSettlementGroup,
                powerPlantGSRN: null,
                address: Address.Create(
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: string.Empty,
                city: string.Empty,
                citySubDivision: string.Empty,
                countryCode: string.Empty,
                postCode: string.Empty,
                floor: string.Empty,
                room: string.Empty,
                municipalityCode: default));
        }

        private static void AssertContainsValidationError<TValidationError>(BusinessRulesValidationResult result)
        {
            Assert.Contains(result.Errors, error => error is TValidationError);
        }

        private static void AssertDoesNotContainValidationError<TValidationError>(BusinessRulesValidationResult result)
        {
            Assert.DoesNotContain(result.Errors, error => error is TValidationError);
        }
    }
}
