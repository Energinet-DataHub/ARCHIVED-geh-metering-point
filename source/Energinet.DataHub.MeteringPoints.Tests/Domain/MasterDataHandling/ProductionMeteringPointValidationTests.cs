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

using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class ProductionMeteringPointValidationTests : TestBase
    {
        [Theory]
        [InlineData("Zero", "Physical", false)]
        [InlineData("One", "Physical", true)]
        [InlineData("One", "Virtual", false)]
        [InlineData("One", "Calculated", false)]
        [InlineData("Two", "Physical", true)]
        [InlineData("Two", "Virtual", false)]
        [InlineData("Two", "Calculated", false)]
        [InlineData("Three", "Physical", true)]
        [InlineData("Three", "Virtual", false)]
        [InlineData("Three", "Calculated", false)]
        [InlineData("Six", "Physical", true)]
        [InlineData("Six", "Virtual", false)]
        [InlineData("Six", "Calculated", false)]
        [InlineData("NinetyNine", "Physical", false)]
        public void Metering_method_must_be_virtual_or_calculated_when_net_settlement_group_is_not_0_or_99(string netSettlementGroup, string meteringMethod, bool expectError)
        {
            var method = EnumerationType.FromName<MeteringMethod>(meteringMethod);
            var meter = method == MeteringMethod.Physical ? "Fake" : string.Empty;
            var details = Builder()
                .WithNetSettlementGroup(netSettlementGroup)
                .WithMeteringConfiguration(meteringMethod, meter)
                .Build();

            AssertError<MeteringMethodDoesNotMatchNetSettlementGroupRuleError>(CheckRules(details), expectError);
        }

        [Fact]
        public void Street_name_is_required()
        {
            var masterData = Builder()
                .WithAddress(streetName: string.Empty)
                .Build();

            AssertContainsValidationError<StreetNameIsRequiredRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Post_code_is_required()
        {
            var masterData = Builder()
                .WithAddress(postCode: string.Empty)
                .Build();

            AssertContainsValidationError<PostCodeIsRequiredRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Geo_info_reference_is_required()
        {
            var masterData = Builder()
                .WithAddress(geoInfoReference: null)
                .Build();

            AssertContainsValidationError<GeoInfoReferenceIsRequiredRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Asset_type_is_required()
        {
            var masterData = Builder()
                .WithAssetType(null!)
                .Build();

            AssertContainsValidationError<AssetTypeIsRequiredRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Powerplant_is_required()
        {
            var masterData = Builder()
                .WithPowerPlant(null!)
                .Build();

            AssertContainsValidationError<PowerPlantRequirementRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Connection_type_is_not_allowed_for_net_settlement_group_0()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Zero.Name)
                .WithConnectionType(ConnectionType.Installation.Name)
                .Build();

            AssertContainsValidationError<ConnectionTypeIsNotAllowedRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Connection_type_is_required_when_net_settlement_group_is_not_0()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithConnectionType(null!)
                .Build();

            AssertContainsValidationError<ConnectionTypeIsRequiredRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Connection_type_must_match_net_settlement_group()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithConnectionType(ConnectionType.Direct.Name)
                .Build();

            AssertContainsValidationError<ConnectionTypeDoesNotMatchNetSettlementGroupRuleError>(CheckRules(masterData));
        }

        private static IMasterDataBuilder Builder() =>
            new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(MeteringPointType.Production))
                .WithAddress(streetName: "Test Street", countryCode: CountryCode.DK)
                .WithNetSettlementGroup(NetSettlementGroup.Two.Name)
                .WithMeteringConfiguration(MeteringMethod.Virtual.Name, string.Empty);

        private static BusinessRulesValidationResult CheckRules(MasterData masterData)
        {
            return new MasterDataValidator().CheckRulesFor(MeteringPointType.Production, masterData);
        }
    }
}
