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
using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Exceptions;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class UpdateTests : TestBase
    {
        [Fact]
        public void Production_obligation_is_unchanged_if_no_value_is_provided()
        {
            var masterData = Builder()
                .WithProductionObligation(true)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithProductionObligation(null)
                .Build();

            Assert.Equal(masterData.ProductionObligation, updatedMasterData.ProductionObligation);
        }

        [Fact]
        public void Production_obligation_is_removed_if_field_is_not_allowed()
        {
            var masterData = Builder()
                .WithProductionObligation(true)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, new List<MasterDataField>
                {
                 new MasterDataField(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                })
                .WithProductionObligation(false)
                .Build();

            Assert.Null(updatedMasterData.ProductionObligation);
        }

        [Fact]
        public void Production_obligation_is_changed()
        {
            var masterData = Builder()
                .WithProductionObligation(true)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithProductionObligation(false)
                .Build();

            Assert.False(updatedMasterData.ProductionObligation);
        }

        [Fact]
        public void Net_settlement_group_input_value_must_be_valid()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Ninetynine.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithNetSettlementGroup("invalid value")
                .Validate();

            AssertContainsValidationError<InvalidNetSettlementGroupValue>(validationResult);
        }

        [Fact]
        public void Net_settlement_group_is_removed_if_field_is_not_allowed()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Ninetynine.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.NetSettlementGroup), Applicability.NotAllowed),
                })
                .WithNetSettlementGroup(NetSettlementGroup.One.Name)
                .Build();

            Assert.Null(updatedMasterData.NetSettlementGroup);
        }

        [Fact]
        public void Net_settlement_group_can_be_removed_if_field_is_optional()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Ninetynine.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.NetSettlementGroup), Applicability.Optional),
                })
                .WithNetSettlementGroup(string.Empty)
                .Build();

            Assert.Null(updatedMasterData.NetSettlementGroup);
        }

        [Fact]
        public void Net_settlement_group_cannot_be_removed_if_field_is_required()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Ninetynine.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.NetSettlementGroup), Applicability.Required),
                })
                .WithNetSettlementGroup(string.Empty)
                .Validate();

            AssertContainsValidationError<NetSettlementGroupIsRequired>(validationResult);
        }

        [Fact]
        public void Net_settlement_group_is_changed()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Ninetynine.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithNetSettlementGroup(NetSettlementGroup.One.Name)
                .Build();

            Assert.Equal(NetSettlementGroup.One, updatedMasterData.NetSettlementGroup);
        }

        [Fact]
        public void Cannot_build_if_validation_error_exists()
        {
            var masterData = Builder()
                .Build();

            var updater = UpdateBuilder(
                masterData,
                new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.EffectiveDate), Applicability.Required),
                });

            Assert.Throws<MasterDataChangeException>(() => updater.Build());
        }

        [Fact]
        public void Effective_date_must_be_set_if_required()
        {
            var masterData = Builder()
                .Build();

            var validationResult = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.EffectiveDate), Applicability.Required),
                })
                .Validate();

            AssertContainsValidationError<EffectiveDateIsRequired>(validationResult);
        }

        [Fact]
        public void Effective_date_must_be_valid()
        {
            var masterData = Builder()
                .Build();

            var effectiveDate = "invalid effective date";
            var validationResult = UpdateBuilder(masterData)
                .EffectiveOn(effectiveDate)
                .Validate();

            Assert.False(validationResult.Success);
        }

        [Fact]
        public void Effective_date_is_set()
        {
            var masterData = Builder()
                .Build();

            var effectiveDate = "2022-01-01T23:00:00Z";
            var updatedMasterData = UpdateBuilder(masterData)
                .EffectiveOn(effectiveDate)
                .Build();

            Assert.Equal(effectiveDate, updatedMasterData.EffectiveDate?.DateInUtc.ToString());
        }

        [Fact]
        public void Capacity_input_value_must_be_valid()
        {
            var masterData = Builder()
                .WithCapacity("100")
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithCapacity("invalid value")
                .Validate();

            Assert.False(validationResult.Success);
        }

        [Fact]
        public void Capacity_is_removed_if_field_is_not_allowed()
        {
            var masterData = Builder()
                .WithCapacity("100")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.Capacity), Applicability.NotAllowed),
                })
                .WithCapacity("1000")
                .Build();

            Assert.Null(updatedMasterData.Capacity);
        }

        [Fact]
        public void Capacity_cannot_be_removed_if_field_is_required()
        {
            var masterData = Builder()
                .WithCapacity("100")
                .Build();

            var validationResult = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.Capacity), Applicability.Required),
                })
                .WithCapacity(string.Empty)
                .Validate();

            AssertContainsValidationError<CapacityIsRequired>(validationResult);
        }

        [Fact]
        public void Capacity_can_be_removed_if_field_is_optional()
        {
            var masterData = Builder()
                .WithCapacity("100")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.Capacity), Applicability.Optional),
                })
                .WithCapacity(string.Empty)
                .Build();

            Assert.Null(updatedMasterData.Capacity);
        }

        [Fact]
        public void Capacity_is_unchanged_if_no_value_is_provided()
        {
            var masterData = Builder()
                .WithCapacity("100")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithCapacity(null)
                .Build();

            Assert.Equal(masterData.Capacity, updatedMasterData.Capacity);
        }

        [Fact]
        public void Capacity_is_changed()
        {
            var masterData = Builder()
                .WithCapacity("100")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithCapacity("1000")
                .Build();

            Assert.Equal(1000, updatedMasterData.Capacity?.Kw);
        }

        [Fact]
        public void Scheduled_meter_reading_date_input_value_must_be_valid()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithScheduledMeterReadingDate("0101")
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithScheduledMeterReadingDate("invalid value")
                .Validate();

            AssertContainsValidationError<InvalidScheduledMeterReadingDateRuleError>("E86", validationResult);
        }

        [Fact]
        public void Scheduled_meter_reading_date_cannot_be_removed_if_field_is_required()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithScheduledMeterReadingDate("0101")
                .Build();

            var validationResult = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.ScheduledMeterReadingDate), Applicability.Required),
                })
                .WithScheduledMeterReadingDate(string.Empty)
                .Validate();

            AssertContainsValidationError<ScheduledMeterReadingDateIsRequired>(validationResult);
        }

        [Fact]
        public void Scheduled_meter_reading_date_can_be_removed_if_field_is_optional()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithScheduledMeterReadingDate("0101")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.ScheduledMeterReadingDate), Applicability.Optional),
                })
                .WithScheduledMeterReadingDate(string.Empty)
                .Build();

            Assert.Null(updatedMasterData.ScheduledMeterReadingDate);
        }

        [Fact]
        public void Scheduled_meter_reading_date_is_unchanged_if_no_value_is_provided()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithScheduledMeterReadingDate("0101")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithScheduledMeterReadingDate(null)
                .Build();

            Assert.Equal(masterData.ScheduledMeterReadingDate, updatedMasterData.ScheduledMeterReadingDate);
        }

        [Fact]
        public void Scheduled_meter_reading_date_is_changed()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithScheduledMeterReadingDate("0101")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithScheduledMeterReadingDate("0102")
                .Build();

            Assert.Equal("0102", updatedMasterData.ScheduledMeterReadingDate?.MonthAndDay);
        }

        [Fact]
        public void Asset_type_input_value_must_be_valid()
        {
            var masterData = Builder()
                .WithAssetType(AssetType.Boiler.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithAssetType("invalid value")
                .Validate();

            AssertContainsValidationError<InvalidAssetTypeValue>("D59", validationResult);
        }

        [Fact]
        public void Asset_type_cannot_be_removed_if_field_is_required()
        {
            var masterData = Builder()
                .WithAssetType(AssetType.Boiler.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.AssetType), Applicability.Required),
                })
                .WithAssetType(string.Empty)
                .Validate();

            AssertContainsValidationError<AssetTypeIsRequired>("D59", validationResult);
        }

        [Fact]
        public void Asset_type_is_removed_if_field_is_not_allowed()
        {
            var masterData = Builder()
                .WithAssetType(AssetType.Boiler.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.AssetType), Applicability.NotAllowed),
                })
                .WithAssetType(AssetType.CombinedCycle.Name)
                .Build();

            Assert.Null(updatedMasterData.AssetType);
        }

        [Fact]
        public void Asset_type_can_be_removed_if_field_is_optional()
        {
            var masterData = Builder()
                .WithAssetType(AssetType.Boiler.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.AssetType), Applicability.Optional),
                })
                .WithAssetType(string.Empty)
                .Build();

            Assert.Null(updatedMasterData.AssetType);
        }

        [Fact]
        public void Asset_type_is_unchanged_if_no_value_is_provided()
        {
            var masterData = Builder()
                .WithAssetType(AssetType.Boiler.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithAssetType(null)
                .Build();

            Assert.Equal(AssetType.Boiler, updatedMasterData.AssetType);
        }

        [Fact]
        public void Asset_type_is_changed()
        {
            var masterData = Builder()
                .WithAssetType(AssetType.Boiler.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithAssetType(AssetType.CombinedCycle.Name)
                .Build();

            Assert.Equal(AssetType.CombinedCycle, updatedMasterData.AssetType);
        }

        [Fact]
        public void Disconnection_type_input_value_must_be_valid()
        {
            var masterData = Builder()
                .WithDisconnectionType(DisconnectionType.Manual.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithDisconnectionType("Invalid value")
                .Validate();

            AssertContainsValidationError<InvalidDisconnectionTypeValue>("D65", validationResult);
        }

        [Fact]
        public void Disconnection_type_is_removed_if_field_is_not_allowed()
        {
            var masterData = Builder()
                .WithDisconnectionType(DisconnectionType.Manual.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(
                    masterData,
                    new List<MasterDataField>()
                    {
                        new MasterDataField(nameof(MasterData.DisconnectionType), Applicability.NotAllowed),
                    })
                .WithDisconnectionType(DisconnectionType.Remote.Name)
                .Build();

            Assert.Null(updatedMasterData.DisconnectionType);
        }

        [Fact]
        public void Disconnection_type_cannot_be_removed_if_required()
        {
            var masterData = Builder()
                .WithDisconnectionType(DisconnectionType.Manual.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.DisconnectionType), Applicability.Required),
                })
                .WithDisconnectionType(string.Empty)
                .Validate();

            AssertContainsValidationError<DisconnectionTypeIsRequired>("D65", validationResult);
        }

        [Fact]
        public void Disconnection_type_is_unchanged_if_no_value_is_provided()
        {
            var masterData = Builder()
                .WithDisconnectionType(DisconnectionType.Manual.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithDisconnectionType(null)
                .Build();

            Assert.Equal(DisconnectionType.Manual, updatedMasterData.DisconnectionType);
        }

        [Fact]
        public void Disconnection_type_can_be_removed_if_optional()
        {
            var masterData = Builder()
                .WithDisconnectionType(DisconnectionType.Manual.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithDisconnectionType(string.Empty)
                .Build();

            Assert.Null(updatedMasterData.DisconnectionType);
        }

        [Fact]
        public void Disconnection_type_is_changed()
        {
            var masterData = Builder()
                .WithDisconnectionType(DisconnectionType.Manual.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithDisconnectionType(DisconnectionType.Remote.Name)
                .Build();

            Assert.Equal(DisconnectionType.Remote, updatedMasterData.DisconnectionType);
        }

        [Fact]
        public void Settlement_method_cannot_be_removed_if_field_is_required()
        {
            var fields = new List<MasterDataField>()
            {
                new MasterDataField(nameof(MasterData.SettlementMethod), Applicability.Required),
            };

            var masterData = Builder(fields)
                .WithSettlementMethod(SettlementMethod.Flex.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData, fields)
                .WithSettlementMethod(string.Empty)
                .Validate();

            AssertContainsValidationError<SettlementMethodIsRequired>(validationResult);
        }

        [Fact]
        public void Settlement_method_can_be_changed_if_field_is_optional()
        {
            var fields = new List<MasterDataField>()
            {
                new MasterDataField(nameof(MasterData.SettlementMethod), Applicability.Optional),
            };

            var masterData = Builder(fields)
                .WithSettlementMethod(SettlementMethod.Flex.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, fields)
                .WithSettlementMethod(string.Empty)
                .Build();

            Assert.Null(updatedMasterData.SettlementMethod);
        }

        [Fact]
        public void Settlement_method_is_unchanged_if_no_value_is_provided()
        {
            var masterData = Builder()
                .WithSettlementMethod(SettlementMethod.Flex.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithSettlementMethod(null)
                .Build();

            Assert.Equal(masterData.SettlementMethod, updatedMasterData.SettlementMethod);
        }

        [Fact]
        public void Settlement_method_is_changed()
        {
            var masterData = Builder()
                .WithSettlementMethod(SettlementMethod.Flex.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithSettlementMethod(SettlementMethod.Profiled.Name)
                .Build();

            Assert.Equal(SettlementMethod.Profiled.Name, updatedMasterData.SettlementMethod?.Name);
        }

        [Fact]
        public void Power_limit_is_unchanged_when_no_value_is_provided()
        {
            var masterData = Builder()
                .WithPowerLimit(100, 100)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithPowerLimit((string?)null, null)
                .Build();

            Assert.Equal(updatedMasterData.PowerLimit.Kwh, masterData.PowerLimit.Kwh);
            Assert.Equal(updatedMasterData.PowerLimit.Ampere, masterData.PowerLimit.Ampere);
        }

        [Fact]
        public void Power_limit_is_cleared_if_empty_value_is_provided()
        {
            var masterData = Builder()
                .WithPowerLimit(100, 100)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithPowerLimit(string.Empty, string.Empty)
                .Build();

            Assert.Null(updatedMasterData.PowerLimit.Kwh);
            Assert.Null(updatedMasterData.PowerLimit.Ampere);
        }

        [Fact]
        public void Power_limit_is_set_if_already_null()
        {
            var masterData = Builder()
                .WithPowerLimit((string?)null, null)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithPowerLimit(200, 300)
                .Build();

            Assert.Equal(200, updatedMasterData.PowerLimit.Kwh);
            Assert.Equal(300, updatedMasterData.PowerLimit.Ampere);
        }

        [Fact]
        public void Power_limit_is_changed()
        {
            var masterData = Builder()
                .WithPowerLimit(100, 100)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithPowerLimit(200, 200)
                .Build();

            Assert.Equal(200, updatedMasterData.PowerLimit.Kwh);
            Assert.Equal(200, updatedMasterData.PowerLimit.Ampere);
        }

        [Fact]
        public void Reading_periodicity_input_value_must_be_valid()
        {
            var masterData = Builder()
                .WithReadingPeriodicity(ReadingOccurrence.Hourly.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithReadingPeriodicity("invalid reading periodicity value")
                .Validate();

            AssertContainsValidationError<InvalidReadingPeriodicityType>(validationResult);
        }

        [Fact]
        public void Reading_periodicity_is_unchanged_if_no_value_is_provided()
        {
            var masterData = Builder()
                .WithReadingPeriodicity(ReadingOccurrence.Hourly.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithReadingPeriodicity(null)
                .Build();

            Assert.Equal(masterData.ReadingOccurrence, updatedMasterData.ReadingOccurrence);
        }

        [Fact]
        public void Reading_periodicity_cannot_be_removed()
        {
            var masterData = Builder()
                .WithReadingPeriodicity(ReadingOccurrence.Hourly.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithReadingPeriodicity(string.Empty)
                .Validate();

            AssertContainsValidationError<MeterReadingPeriodicityIsRequired>(validationResult);
        }

        [Fact]
        public void Reading_periodicity_is_changed()
        {
            var masterData = Builder()
                .WithReadingPeriodicity(ReadingOccurrence.Hourly.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithReadingPeriodicity(ReadingOccurrence.Monthly.Name)
                .Build();

            Assert.Equal(ReadingOccurrence.Monthly, updatedMasterData.ReadingOccurrence);
        }

        [Fact]
        public void Power_plant_input_value_must_be_valid()
        {
            var masterData = Builder()
                .WithPowerPlant("571234567891234568")
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithPowerPlant("invalid gsrn number string")
                .Validate();

            Assert.False(validationResult.Success);
        }

        [Fact]
        public void Power_plant_cannot_be_removed_if_field_is_required()
        {
            var fields = new List<MasterDataField>()
            {
                new MasterDataField(nameof(MasterData.PowerPlantGsrnNumber), Applicability.Required),
            };

            var masterData = Builder(fields)
                .WithPowerPlant("571234567891234568")
                .Build();

            var validationResult = UpdateBuilder(masterData, fields)
                .WithPowerPlant(string.Empty)
                .Validate();

            AssertContainsValidationError<PowerPlantIsRequired>("D57", validationResult);
        }

        [Fact]
        public void Power_plant_can_be_removed_if_field_is_optional()
        {
            var fields = new List<MasterDataField>()
            {
                new MasterDataField(nameof(MasterData.PowerPlantGsrnNumber), Applicability.Optional),
            };

            var masterData = Builder(fields)
                .WithPowerPlant("571234567891234568")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, fields)
                .WithPowerPlant(string.Empty)
                .Build();

            Assert.Null(updatedMasterData.PowerPlantGsrnNumber);
        }

        [Fact]
        public void Power_plant_is_not_changed_if_field_is_not_allowed()
        {
            var fields = new List<MasterDataField>()
            {
                new MasterDataField(nameof(MasterData.PowerPlantGsrnNumber), Applicability.NotAllowed),
            };

            var masterData = Builder(fields)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, fields)
                .WithPowerPlant("570851247381952311")
                .Build();

            Assert.Equal(masterData.PowerPlantGsrnNumber, updatedMasterData.PowerPlantGsrnNumber);
        }

        [Fact]
        public void Power_plant_is_unchanged_if_no_value_is_provided()
        {
            var masterData = Builder()
                .WithPowerPlant("571234567891234568")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithPowerPlant(null)
                .Build();

            Assert.Equal(masterData.PowerPlantGsrnNumber, updatedMasterData.PowerPlantGsrnNumber);
        }

        [Fact]
        public void Power_plant_is_changed()
        {
            var masterData = Builder()
                .WithPowerPlant("571234567891234568")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithPowerPlant("570851247381952311")
                .Build();

            Assert.Equal("570851247381952311", updatedMasterData.PowerPlantGsrnNumber?.Value);
        }

        [Fact]
        public void Unit_type_input_value_must_valid()
        {
            var masterData = Builder()
                .WithMeasurementUnitType(MeasurementUnitType.Ampere.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithMeasurementUnitType("invalid unit type value")
                .Validate();

            AssertContainsValidationError<InvalidUnitTypeValue>(validationResult);
        }

        [Fact]
        public void Unit_type_cannot_be_removed()
        {
            var masterData = Builder()
                .WithMeasurementUnitType(MeasurementUnitType.Ampere.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.UnitType), Applicability.Required),
                })
                .WithMeasurementUnitType(string.Empty)
                .Validate();

            AssertContainsValidationError<UnitTypeIsRequired>(validationResult);
        }

        [Fact]
        public void Unit_type_is_unchanged_if_no_value_is_provided()
        {
            var masterData = Builder()
                .WithMeasurementUnitType(MeasurementUnitType.Ampere.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithMeasurementUnitType(null)
                .Build();

            Assert.Equal(masterData.UnitType, updatedMasterData.UnitType);
        }

        [Fact]
        public void Unit_type_is_changed()
        {
            var masterData = Builder()
                .WithMeasurementUnitType(MeasurementUnitType.Ampere.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithMeasurementUnitType(MeasurementUnitType.KWh.Name)
                .Build();

            Assert.Equal(MeasurementUnitType.KWh, updatedMasterData.UnitType);
        }

        [Fact]
        public void Address_must_be_valid()
        {
            var masterData = Builder()
                .WithAddress(
                    streetName: "test street",
                    streetCode: "5",
                    buildingNumber: "5",
                    city: "Test City",
                    citySubDivision: "test",
                    postCode: "8000",
                    CountryCode.DK,
                    floor: "1",
                    room: "1",
                    municipalityCode: 500,
                    isActual: true,
                    geoInfoReference: Guid.NewGuid().ToString(),
                    locationDescription: "Test location")
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithAddress(streetCode: "Invalid street code")
                .Validate();

            Assert.False(validationResult.Success);
        }

        [Fact]
        public void Address_is_changed()
        {
            var masterData = Builder()
                .WithAddress(
                    streetName: "test street",
                    streetCode: "5",
                    buildingNumber: "5",
                    city: "Test City",
                    citySubDivision: "test",
                    postCode: "8000",
                    CountryCode.DK,
                    floor: "1",
                    room: "1",
                    municipalityCode: 500,
                    isActual: true,
                    geoInfoReference: Guid.NewGuid().ToString(),
                    locationDescription: "Test location")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithAddress("New Street")
                .Build();

            Assert.Equal("New Street", updatedMasterData.Address.StreetName);
            Assert.Equal(masterData.Address.StreetCode, updatedMasterData.Address.StreetCode);
        }

        [Fact]
        public void Metering_configuration_must_be_valid()
        {
            var masterData = Builder()
                .WithMeteringConfiguration(MeteringMethod.Physical.Name, "1")
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithMeteringConfiguration(null, string.Empty)
                .Validate();

            Assert.False(validationResult.Success);
        }

        [Fact]
        public void Metering_method_input_value_must_be_Valid()
        {
            var masterData = Builder()
                .WithMeteringConfiguration(MeteringMethod.Virtual.Name, null)
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithMeteringConfiguration("invalid value", "2")
                .Validate();

            AssertContainsValidationError<InvalidMeteringMethodValue>(validationResult);
        }

        [Fact]
        public void Metering_method_cannot_be_removed()
        {
            var masterData = Builder()
                .WithMeteringConfiguration(MeteringMethod.Virtual.Name, null)
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithMeteringConfiguration(string.Empty, "2")
                .Validate();

            AssertContainsValidationError<MeteringMethodIsRequired>(validationResult);
        }

        [Fact]
        public void Meter_number_input_value_must_be_Valid()
        {
            var masterData = Builder()
                .WithMeteringConfiguration(MeteringMethod.Physical.Name, "1")
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithMeteringConfiguration(null, "12345678901234567890")
                .Validate();

            AssertContainsValidationError<InvalidMeterIdRuleError>(validationResult);
        }

        [Fact]
        public void Meter_number_is_ignored_if_not_applicable_according_to_metering_method()
        {
            var masterData = Builder()
                .WithMeteringConfiguration(MeteringMethod.Virtual.Name, null)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithMeteringConfiguration(MeteringMethod.Calculated.Name, "2")
                .Build();

            Assert.Equal(MeteringMethod.Calculated, updatedMasterData.MeteringConfiguration.Method);
            Assert.Equal(string.Empty, updatedMasterData.MeteringConfiguration.Meter.Value);
        }

        [Fact]
        public void Meter_number_is_unchanged_if_no_value_is_provided()
        {
            var masterData = Builder()
                .WithMeteringConfiguration(MeteringMethod.Physical.Name, "1")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithMeteringConfiguration(null, null)
                .Build();

            Assert.Equal(masterData.MeteringConfiguration.Method, updatedMasterData.MeteringConfiguration.Method);
            Assert.Equal(masterData.MeteringConfiguration.Meter.Value, updatedMasterData.MeteringConfiguration.Meter.Value);
        }

        [Fact]
        public void Meter_number_is_changed()
        {
            var masterData = Builder()
                .WithMeteringConfiguration(MeteringMethod.Physical.Name, "1")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithMeteringConfiguration(null, "2")
                .Build();

            Assert.Equal(masterData.MeteringConfiguration.Method, updatedMasterData.MeteringConfiguration.Method);
            Assert.Equal("2", updatedMasterData.MeteringConfiguration.Meter.Value);
        }

        [Fact]
        public void Meter_number_is_removed_if_changing_method_from_physical()
        {
            var masterData = Builder()
                .WithMeteringConfiguration(MeteringMethod.Physical.Name, "1")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithMeteringConfiguration(MeteringMethod.Calculated.Name, null)
                .Build();

            Assert.Equal(MeteringMethod.Calculated, updatedMasterData.MeteringConfiguration.Method);
            Assert.Equal(string.Empty, updatedMasterData.MeteringConfiguration.Meter.Value);
        }

        [Fact]
        public void Connection_type_cannot_be_removed_if_field_is_required()
        {
            var masterData = Builder()
                .WithConnectionType(ConnectionType.Direct.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.ConnectionType), Applicability.Required),
                })
                .WithConnectionType(string.Empty)
                .Validate();

            AssertContainsValidationError<ConnectionTypeIsRequired>(validationResult);
        }

        [Fact]
        public void Connection_type_is_removed_if_field_is_not_allowed()
        {
            var masterData = Builder()
                .WithConnectionType(ConnectionType.Direct.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.ConnectionType), Applicability.NotAllowed),
                })
                .WithConnectionType(ConnectionType.Installation.Name)
                .Build();

            Assert.Null(updatedMasterData.ConnectionType);
        }

        [Fact]
        public void Connection_type_input_value_must_be_valid()
        {
            var masterData = Builder()
                .WithConnectionType(ConnectionType.Direct.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithConnectionType("invalid value")
                .Validate();

            AssertError<InvalidConnectionTypeValue>("D66", validationResult, true);
        }

        [Fact]
        public void Connection_type_is_unchanged()
        {
            var masterData = Builder()
                .WithConnectionType(ConnectionType.Direct.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithConnectionType(null)
                .Build();

            Assert.Equal(masterData.ConnectionType, updatedMasterData.ConnectionType);
        }

        [Fact]
        public void Connection_type_is_removed_if_optional()
        {
            var masterData = Builder()
                .WithConnectionType(ConnectionType.Direct.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithConnectionType(string.Empty)
                .Build();

            Assert.Null(updatedMasterData.ConnectionType);
        }

        [Fact]
        public void Connection_type_is_changed()
        {
            var masterData = Builder()
                .WithConnectionType(ConnectionType.Direct.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithConnectionType(ConnectionType.Installation.Name)
                .Build();

            Assert.Equal(ConnectionType.Installation, updatedMasterData.ConnectionType);
        }

        [Fact]
        public void Connection_type_is_removed_when_changing_net_settlement_group_to_0()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.One.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithNetSettlementGroup(NetSettlementGroup.Zero.Name)
                .Build();

            Assert.Null(updatedMasterData.ConnectionType);
        }

        [Fact]
        public void Scheduled_meter_reading_date_is_removed_when_changing_net_settlement_group_6_to_0()
        {
            var masterData = Builder()
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithScheduledMeterReadingDate("0101")
                .Build();

            var updatedMasterData = UpdateBuilder(masterData)
                .WithNetSettlementGroup(NetSettlementGroup.Zero.Name)
                .Build();

            Assert.Null(updatedMasterData.ScheduledMeterReadingDate);
        }

        [Fact]
        public void Product_type_input_value_must_valid()
        {
            var masterData = Builder()
                .WithProductType(ProductType.Tariff.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithProductType("invalid value")
                .Validate();

            AssertContainsValidationError<InvalidProductTypeValue>(validationResult);
        }

        [Fact]
        public void Product_type_is_removed_if_field_is_not_allowed()
        {
            var masterData = Builder()
                .WithProductType(ProductType.Tariff.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.ProductType), Applicability.NotAllowed),
                })
                .WithProductType(ProductType.EnergyActive.Name)
                .Build();

            Assert.Null(updatedMasterData.ProductType);
        }

        [Fact]
        public void Product_type_can_be_removed_if_field_is_optional()
        {
            var masterData = Builder()
                .WithProductType(ProductType.Tariff.Name)
                .Build();

            var updatedMasterData = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.ProductType), Applicability.Optional),
                })
                .WithProductType(string.Empty)
                .Build();

            Assert.Null(updatedMasterData.ProductType);
        }

        [Fact]
        public void Product_type_cannot_be_removed_if_field_is_required()
        {
            var masterData = Builder()
                .WithProductType(ProductType.Tariff.Name)
                .Build();

            var validationResult = UpdateBuilder(masterData, new List<MasterDataField>()
                {
                    new MasterDataField(nameof(MasterData.ProductType), Applicability.Required),
                })
                .WithProductType(string.Empty)
                .Validate();

            AssertContainsValidationError<ProductTypeIsRequired>(validationResult);
        }

        [Fact]
        public void Product_type_is_changed()
        {
            var updatedMasterData = UpdateBuilder(Builder().Build())
                .WithProductType(ProductType.Tariff.Name)
                .Build();

            Assert.Equal(ProductType.Tariff, updatedMasterData.ProductType);
        }

        [Fact]
        public void Product_type_is_unchanged_if_no_value_is_provided()
        {
            var currentMasterData = Builder().Build();

            var updatedMasterData = UpdateBuilder(currentMasterData)
                    .Build();

            Assert.Equal(currentMasterData.ProductType, updatedMasterData.ProductType);
        }

        [Theory]
        [InlineData(null)]
        public void Product_type_is_unchanged_if_a_null_value_is_provided(string? providedProductType)
        {
            var currentMasterData = Builder().Build();
            var updatedMasterData = UpdateBuilder(currentMasterData)
                    .WithProductType(providedProductType)
                    .Build();

            Assert.Equal(currentMasterData.ProductType, updatedMasterData.ProductType);
        }

        private static IMasterDataBuilder Builder(IEnumerable<MasterDataField>? fieldConfiguration = null)
        {
            return new MasterDataBuilder(fieldConfiguration ?? new List<MasterDataField>())
                .WithNetSettlementGroup(NetSettlementGroup.One.Name)
                .WithSettlementMethod(SettlementMethod.Flex.Name)
                .WithScheduledMeterReadingDate("0101")
                .WithCapacity("1")
                .WithAddress(
                    SampleData.StreetName,
                    SampleData.StreetCode,
                    string.Empty,
                    SampleData.CityName,
                    string.Empty,
                    SampleData.PostCode,
                    CountryCode.DK,
                    string.Empty,
                    string.Empty,
                    default,
                    isActual: true,
                    geoInfoReference: Guid.NewGuid().ToString(),
                    null)
                .WithAssetType(AssetType.GasTurbine.Name)
                .WithPowerPlant(SampleData.PowerPlant)
                .WithReadingPeriodicity(ReadingOccurrence.Hourly.Name)
                .WithPowerLimit(0, 0)
                .EffectiveOn(SampleData.EffectiveDate)
                .WithDisconnectionType(DisconnectionType.Manual.Name)
                .WithConnectionType(ConnectionType.Installation.Name)
                .WithMeteringConfiguration(MeteringMethod.Virtual.Name, string.Empty)
                .WithProductType(ProductType.EnergyActive.Name)
                .WithMeasurementUnitType(MeasurementUnitType.KWh.Name);
        }

        private static IMasterDataBuilder BuilderFor(MeteringPointType meteringPointType) =>
            new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(meteringPointType))
                .WithMeteringConfiguration(MeteringMethod.Calculated.Name, null);

        private static MasterDataUpdater UpdateBuilder(MasterData current, IEnumerable<MasterDataField>? fieldConfiguration = null)
        {
            return new MasterDataUpdater(fieldConfiguration ?? new List<MasterDataField>(), current);
        }
    }
}
