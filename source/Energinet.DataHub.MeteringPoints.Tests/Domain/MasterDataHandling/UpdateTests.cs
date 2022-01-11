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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    #pragma warning disable
    [UnitTest]
    public class UpdateTests : TestBase
    {
        [Fact]
        public void Metering_configuration_must_be_valid()
        {
            var masterData = Builder()
                .WithMeteringConfiguration(MeteringMethod.Physical.Name, "1")
                .Build();

            var validationResult = UpdateBuilder(masterData)
                .WithMeteringConfiguration(null, "")
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
            Assert.Equal("", updatedMasterData.MeteringConfiguration.Meter.Value);
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

            Assert.Equal(null, updatedMasterData.ConnectionType);
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

            Assert.Equal(null, updatedMasterData.ConnectionType);
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

            Assert.Equal(null, updatedMasterData.ConnectionType);
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

            Assert.Equal(null, updatedMasterData.ScheduledMeterReadingDate);
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

        private IMasterDataBuilder Builder(IEnumerable<MasterDataField> fieldConfiguration = null)
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

        private MasterDataUpdater UpdateBuilder(MasterData current, IEnumerable<MasterDataField> fieldConfiguration = null)
        {
            return new MasterDataUpdater(fieldConfiguration ?? new List<MasterDataField>(), current);
        }
    }

    public class MasterDataUpdater : MasterDataBuilderBase, IMasterDataBuilder
    {
        private readonly List<ValidationError> _validationErrors = new();

        public MasterDataUpdater(IEnumerable<MasterDataField> fields, MasterData currentMasterData)
            :base(fields)
        {
            SetValue(nameof(MasterData.ProductType), currentMasterData.ProductType);
            SetValue(nameof(MasterData.NetSettlementGroup), currentMasterData.NetSettlementGroup);
            SetValue(nameof(MasterData.ConnectionType), currentMasterData.ConnectionType);
            SetValue(nameof(MasterData.MeteringConfiguration), currentMasterData.MeteringConfiguration);
        }

        public BusinessRulesValidationResult Validate()
        {
            _validationErrors.Clear();
            _validationErrors.AddRange(AllValueValidationErrors());

            return new BusinessRulesValidationResult(_validationErrors);
        }

        private void RemoveValueIfNotApplicable<T>(string valueName, Func<bool> rule)
        {
            if (rule())
            {
                SetValue<T>(valueName, default(T));
            }
        }

        public MasterData Build()
        {
            RemoveValueIfNotApplicable<ScheduledMeterReadingDate>(nameof(MasterData.ScheduledMeterReadingDate),
                () => GetValue<NetSettlementGroup>(nameof(MasterData.NetSettlementGroup)) != NetSettlementGroup.Six);
            RemoveValueIfNotApplicable<ConnectionType>(nameof(MasterData.ConnectionType),
                () => GetValue<NetSettlementGroup>(nameof(MasterData.NetSettlementGroup)) == NetSettlementGroup.Zero);

            return new MasterData(
                productType: GetValue<ProductType>(nameof(MasterData.ProductType)),
                unitType: GetValue<MeasurementUnitType>(nameof(MasterData.UnitType)),
                assetType: GetValue<AssetType>(nameof(MasterData.AssetType)),
                readingOccurrence: GetValue<ReadingOccurrence>(nameof(MasterData.ReadingOccurrence)),
                powerLimit: GetValue<PowerLimit>(nameof(MasterData.PowerLimit)),
                powerPlantGsrnNumber: GetValue<GsrnNumber>(nameof(MasterData.PowerPlantGsrnNumber)),
                effectiveDate: GetValue<EffectiveDate>(nameof(MasterData.EffectiveDate)),
                capacity: GetValue<Capacity>(nameof(MasterData.Capacity)),
                address: GetValue<Address>(nameof(MasterData.Address)),
                meteringConfiguration: GetValue<MeteringConfiguration>(nameof(MasterData.MeteringConfiguration)),
                settlementMethod: GetValue<SettlementMethod>(nameof(MasterData.SettlementMethod)),
                scheduledMeterReadingDate: GetValue<ScheduledMeterReadingDate>(nameof(MasterData.ScheduledMeterReadingDate)),
                connectionType: GetValue<ConnectionType>(nameof(MasterData.ConnectionType)),
                disconnectionType: GetValue<DisconnectionType>(nameof(MasterData.DisconnectionType)),
                netSettlementGroup: GetValue<NetSettlementGroup>(nameof(MasterData.NetSettlementGroup)),
                productionObligation: GetValue<bool?>(nameof(MasterData.ProductionObligation)));
        }

        public IMasterDataBuilder WithNetSettlementGroup(string netSettlementGroup)
        {
            SetValueIfValid(
                nameof(MasterData.NetSettlementGroup),
                () =>
                {
                    return EnumerationType.GetAll<NetSettlementGroup>()
                        .Select(item => item.Name)
                        .Contains(netSettlementGroup) == false ? BusinessRulesValidationResult.Failure(new InvalidSettlementGroupValue(netSettlementGroup)) : BusinessRulesValidationResult.Valid();
                },
                () => EnumerationType.FromName<NetSettlementGroup>(netSettlementGroup));
            return this;
        }

        public IMasterDataBuilder WithMeteringConfiguration(string? method, string? meterNumber)
        {
            var currentMeterConfiguration = GetValue<MeteringConfiguration>(nameof(MasterData.MeteringConfiguration));
            SetValueIfValid(
                nameof(MasterData.MeteringConfiguration),
                () =>
                {
                    if (method is not null)
                    {
                        if (EnumerationType.GetAll<MeteringMethod>()
                            .Select(item => item.Name)
                            .Contains(method) == false)
                        {
                            return BusinessRulesValidationResult.Failure(new InvalidMeteringMethodValue(method));
                        }
                    }

                    if (string.IsNullOrWhiteSpace(meterNumber) == false)
                    {
                        var meterValidationResult = MeterId.CheckRules(meterNumber);
                        if (meterValidationResult.Success == false)
                        {
                            return BusinessRulesValidationResult.Failure(meterValidationResult.Errors.ToArray());
                        }
                    }

                    var meteringMethod = method is null
                        ? currentMeterConfiguration.Method
                        : EnumerationType.FromName<MeteringMethod>(method);
                    BusinessRulesValidationResult validationResult;
                    if (meteringMethod == MeteringMethod.Physical)
                    {
                        validationResult = MeteringConfiguration.CheckRules(meteringMethod, string.IsNullOrEmpty(meterNumber) ? MeterId.Empty() : MeterId.Create(meterNumber));
                    }
                    else
                    {
                        validationResult = MeteringConfiguration.CheckRules(meteringMethod, MeterId.Empty());
                    }

                    if (validationResult.Success == false)
                    {
                        return BusinessRulesValidationResult.Failure(validationResult.Errors.ToArray());
                    }

                    return BusinessRulesValidationResult.Valid();
                },
                () =>
                {
                    var meteringMethod = string.IsNullOrEmpty(method) ? currentMeterConfiguration.Method : EnumerationType.FromName<MeteringMethod>(method);
                    var meterId = meteringMethod == MeteringMethod.Physical ? string.IsNullOrEmpty(meterNumber)
                        ? currentMeterConfiguration.Meter : MeterId.Create(meterNumber) : MeterId.Empty();
                    return MeteringConfiguration.Create(meteringMethod, meterId);
                });
            return this;
        }

        public IMasterDataBuilder WithAddress(string? streetName = null, string? streetCode = null, string? buildingNumber = null,
            string? city = null, string? citySubDivision = null, string? postCode = null, CountryCode? countryCode = null,
            string? floor = null, string? room = null, int? municipalityCode = null, bool? isActual = null,
            Guid? geoInfoReference = null, string? locationDescription = null)
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder WithMeasurementUnitType(string? measurementUnitType)
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder WithPowerPlant(string? gsrnNumber)
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder WithReadingPeriodicity(string? readingPeriodicity)
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder WithPowerLimit(int kwh, int ampere)
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder WithSettlementMethod(string? settlementMethod)
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder WithDisconnectionType(string? disconnectionType)
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder WithAssetType(string? assetType)
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder WithScheduledMeterReadingDate(string? scheduledMeterReadingDate)
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder WithCapacity(double? capacity)
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder EffectiveOn(string? effectiveDate)
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder WithProductType(string? productType)
        {
            if (productType is null)
            {
                return this;
            }

            SetValueIfValid(nameof(MasterData.ProductType),
                BusinessRulesValidationResult.Valid,
                () => EnumerationType.FromName<ProductType>(productType));

            return this;
        }

        public IMasterDataBuilder WithConnectionType(string? connectionType)
        {
            if (connectionType is null) return this;
            if (connectionType == string.Empty) SetValue<ConnectionType>(nameof(MasterData.ConnectionType), null);
            if (connectionType != string.Empty) SetValueIfValid(nameof(MasterData.ConnectionType),
                () =>
                {
                    return EnumerationType.GetAll<ConnectionType>()
                        .Select(item => item.Name)
                        .Contains(connectionType) == false ? BusinessRulesValidationResult.Failure(new InvalidConnectionTypeValue(connectionType)) : BusinessRulesValidationResult.Valid();
                },
                () => EnumerationType.FromName<ConnectionType>(connectionType));
            return this;

        }

        public IMasterDataBuilder WithProductionObligation(bool? productionObligation)
        {
            throw new NotImplementedException();
        }

        private void SetValueIfValid<T>(string valueName, Func<BusinessRulesValidationResult> validator, Func<T> creator)
        {
            var valueItem = GetMasterValueItem<T>(valueName);
            valueItem.SetValue(validator, creator);
        }
    }
}
