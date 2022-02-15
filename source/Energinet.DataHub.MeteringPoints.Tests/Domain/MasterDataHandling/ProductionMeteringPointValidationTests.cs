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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class ProductionMeteringPointValidationTests : TestBase
    {
        [Theory]
        [InlineData(nameof(MeasurementUnitType.KWh), false)]
        [InlineData(nameof(MeasurementUnitType.Ampere), true)]
        public void Unit_type_must_be_kwh(string measurementUnitType, bool expectError)
        {
            var masterData = Builder()
                .WithMeasurementUnitType(measurementUnitType)
                .Build();

            AssertError<UnitTypeIsNotValidForMeteringPointType>(CheckRules(masterData), expectError);
        }

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

            AssertError<MeteringMethodDoesNotMatchNetSettlementGroupRuleError>("D37", CheckRules(details), expectError);
        }

        [Fact]
        public void Street_name_is_required()
        {
            var masterData = Builder()
                .WithAddress(streetName: string.Empty)
                .Build();

            AssertContainsValidationError<StreetNameIsRequiredRuleError>("E86", CheckRules(masterData));
        }

        [Fact]
        public void Post_code_is_required()
        {
            var masterData = Builder()
                .WithAddress(postCode: string.Empty)
                .Build();

            AssertContainsValidationError<PostCodeIsRequiredRuleError>("E86", CheckRules(masterData));
        }

        [Fact]
        public void City_is_required()
        {
            var masterData = Builder()
                .WithAddress(city: string.Empty)
                .Build();

            AssertContainsValidationError<CityIsRequiredRuleError>("E86", CheckRules(masterData));
        }

        [Fact]
        public void Geo_info_reference_is_required()
        {
            var masterData = Builder()
                .WithAddress(geoInfoReference: null)
                .Build();

            AssertContainsValidationError<GeoInfoReferenceIsRequiredRuleError>("E86", CheckRules(masterData));
        }

        [Fact]
        public void Asset_type_is_required()
        {
            var masterData = Builder()
                .WithAssetType(null!)
                .Build();

            AssertContainsValidationError<AssetTypeIsRequired>("D59", CheckRules(masterData));
        }

        [Fact]
        public void Powerplant_is_required()
        {
            var masterData = Builder()
                .WithPowerPlant(null!)
                .Build();

            AssertContainsValidationError<PowerPlantIsRequired>("D57", CheckRules(masterData));
        }

        [Fact]
        public void Connection_type_is_not_allowed_for_net_settlement_group_0()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Zero.Name)
                .WithConnectionType(ConnectionType.Installation.Name)
                .Build();

            AssertContainsValidationError<ConnectionTypeIsNotAllowedRuleError>("D02", CheckRules(masterData));
        }

        [Fact]
        public void Connection_type_is_required_when_net_settlement_group_is_not_0()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithConnectionType(null!)
                .Build();

            AssertContainsValidationError<ConnectionTypeIsRequiredRuleError>("D66", CheckRules(masterData));
        }

        [Fact]
        public void Connection_type_must_match_net_settlement_group()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithConnectionType(ConnectionType.Direct.Name)
                .Build();

            AssertContainsValidationError<ConnectionTypeDoesNotMatchNetSettlementGroupRuleError>("D55", CheckRules(masterData));
        }

        [Fact]
        public void Meter_reading_occurrence_must_be_quarterly_or_hourly()
        {
            var masterData = Builder()
                .WithReadingPeriodicity(ReadingOccurrence.Yearly.Name)
                .Build();

            AssertContainsValidationError<InvalidMeterReadingOccurrenceRuleError>(CheckRules(masterData));
        }

        [Theory]
        [InlineData(nameof(ProductType.EnergyActive), false)]
        [InlineData(nameof(ProductType.FuelQuantity), true)]
        public void Product_type_must_be_correct(string productType, bool expectError)
        {
            var masterData = Builder()
                .WithProductType(productType)
                .Build();

            AssertError<InvalidProductType>(CheckRules(masterData), expectError);
        }

        private static IMasterDataBuilder Builder() =>
            new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(MeteringPointType.Production))
                .WithReadingPeriodicity(ReadingOccurrence.Quarterly.Name)
                .WithAddress(streetName: "Test Street", countryCode: CountryCode.DK)
                .WithNetSettlementGroup(NetSettlementGroup.Two.Name)
                .WithMeteringConfiguration(MeteringMethod.Virtual.Name, string.Empty);

        private static BusinessRulesValidationResult CheckRules(MasterData masterData)
        {
            return new MasterDataValidator(
                new ProductionMeteringPointValidator())
                .CheckRulesFor(MeteringPointType.Production, masterData);
        }
    }
}
