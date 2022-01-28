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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;
using PowerPlantIsRequiredForNetSettlementGroupRuleError = Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Consumption.Rules.PowerPlantIsRequiredForNetSettlementGroupRuleError;
using StreetNameIsRequiredRuleError = Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules.StreetNameIsRequiredRuleError;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class ConsumptionMeteringPointValidationTests : TestBase
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
        [InlineData(nameof(ProductType.EnergyActive), false)]
        [InlineData(nameof(ProductType.FuelQuantity), true)]
        public void Product_type_must_be_correct(string productType, bool expectError)
        {
            var masterData = Builder()
                .WithProductType(productType)
                .Build();

            AssertError<InvalidProductType>(CheckRules(masterData), expectError);
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

            AssertError<MeteringMethodDoesNotMatchNetSettlementGroupRuleError>(CheckRules(details), expectError);
        }

        [Fact]
        public void Day_of_scheduled_meter_reading_date_must_be_01_when_net_settlement_group_is_6()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithScheduledMeterReadingDate("0512")
                .Build();

            AssertContainsValidationError<InvalidScheduledMeterReadingDateNetSettlementGroupRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Scheduled_meter_reading_date_is_not_allowed_for_other_than_net_settlement_group_6()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.One.Name)
                .WithScheduledMeterReadingDate("0512")
                .Build();

            AssertContainsValidationError<ScheduledMeterReadingDateNotAllowedRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Scheduled_meter_reading_date_cannot_be_changed()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.Consumption, MasterDataBuilderForConsumption()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithScheduledMeterReadingDate("0101"));

            var updatedMasterData = Updater(meteringPoint)
                .WithScheduledMeterReadingDate("0201")
                .Build();

            AssertError<ScheduledMeterReadingDateCannotBeChanged>(meteringPoint.CanUpdateMasterData(updatedMasterData, CreateValidator()));
        }

        [Fact]
        public void Powerplant_GSRN_is_required_when_netsettlementgroup_is_other_than_0_or_99()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithPowerPlant(null!)
                .Build();

            AssertContainsValidationError<PowerPlantIsRequiredForNetSettlementGroupRuleError>(CheckRules(masterData));
        }

        [Theory]
        [InlineData(nameof(NetSettlementGroup.Zero))]
        [InlineData(nameof(NetSettlementGroup.Ninetynine))]
        public void Powerplant_GSRN_is_not_required_when_netsettlementgroup_is_0_or_99(string netSettlementGroupName)
        {
            var masterData = Builder()
                .WithNetSettlementGroup(netSettlementGroupName)
                .WithPowerPlant(null!)
                .Build();

            AssertDoesNotContainValidationError<PowerPlantIsRequiredForNetSettlementGroupRuleError>(CheckRules(masterData));
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
        public void City_is_required()
        {
            var masterData = Builder()
                .WithAddress(city: string.Empty)
                .Build();

            AssertContainsValidationError<CityIsRequiredRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Capacity_is_required_for_all_net_settlement_groups_but_0()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithCapacity(null!)
                .Build();

            AssertContainsValidationError<CapacityIsRequiredRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Asset_type_is_required_for_net_settlement_groups_other_than_0()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithAssetType(null!)
                .Build();

            AssertContainsValidationError<AssetTypeIsRequired>(CheckRules(masterData));
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

        [Theory]
        [InlineData(nameof(NetSettlementGroup.Six))]
        [InlineData(nameof(NetSettlementGroup.Three))]
        public void Connection_type_must_be_installation(string netSettlementGroup)
        {
            var masterData = Builder()
                .WithNetSettlementGroup(netSettlementGroup)
                .WithConnectionType(ConnectionType.Direct.Name)
                .Build();

            AssertContainsValidationError<ConnectionTypeDoesNotMatchNetSettlementGroupRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Meter_reading_occurrence_must_be_quarterly_or_hourly()
        {
            var masterData = Builder()
                .WithReadingPeriodicity(ReadingOccurrence.Yearly.Name)
                .Build();

            AssertContainsValidationError<InvalidMeterReadingOccurrenceRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Settlement_method_of_profiled_is_not_allowed()
        {
            var masterData = Builder()
                .WithSettlementMethod(SettlementMethod.Profiled.Name)
                .Build();

            AssertContainsValidationError<InvalidSettlementMethodRuleError>(CheckRules(masterData));
        }

        [Fact]
        public void Settlement_method_is_required()
        {
            var builder = Builder()
                .WithSettlementMethod(string.Empty);

            AssertContainsValidationError<SettlementMethodIsRequired>(builder.Validate());
        }

        private static IMasterDataBuilder Builder() =>
            new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(MeteringPointType.Consumption))
                .WithReadingPeriodicity(ReadingOccurrence.Quarterly.Name)
                .WithAddress(streetName: "Test Street", countryCode: CountryCode.DK)
                .WithNetSettlementGroup(NetSettlementGroup.Two.Name)
                .WithMeteringConfiguration(MeteringMethod.Virtual.Name, string.Empty);

        private static MasterDataUpdater Updater(MeteringPoint meteringPoint) => new MasterDataUpdater(
            new MasterDataFieldSelector().GetMasterDataFieldsFor(MeteringPointType.Consumption),
            meteringPoint.MasterData);

        private static BusinessRulesValidationResult CheckRules(MasterData masterData)
        {
            return new MasterDataValidator(new ConsumptionMeteringPointValidator()).CheckRulesFor(MeteringPointType.Consumption, masterData);
        }

        private static MasterDataValidator CreateValidator() =>
            new MasterDataValidator(
                new ConsumptionMeteringPointValidator());
    }
}
