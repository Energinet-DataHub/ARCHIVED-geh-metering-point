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
using BenchmarkDotNet.Reports;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class UpdateTests : TestBase
    {
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
                    geoInfoReference: Guid.NewGuid(),
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
                    geoInfoReference: Guid.NewGuid(),
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

            AssertError<InvalidConnectionTypeValue>(validationResult, true);
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
                .WithCapacity(1)
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
                    geoInfoReference: Guid.NewGuid(),
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

        private static MasterDataUpdater UpdateBuilder(MasterData current, IEnumerable<MasterDataField>? fieldConfiguration = null)
        {
            return new MasterDataUpdater(fieldConfiguration ?? new List<MasterDataField>(), current);
        }
    }
}
