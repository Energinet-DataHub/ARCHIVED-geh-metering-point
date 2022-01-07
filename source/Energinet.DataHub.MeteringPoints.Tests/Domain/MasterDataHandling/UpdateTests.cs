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
        [InlineData("")]
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
        private string? _productType;
        private readonly List<MasterDataValue> _values = new()
        {
            new MasterDataValue(nameof(MasterData.ProductType), typeof(ProductType), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.UnitType), typeof(MeasurementUnitType), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.AssetType), typeof(AssetType), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.SettlementMethod), typeof(SettlementMethod), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.ScheduledMeterReadingDate), typeof(ScheduledMeterReadingDate), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.MeteringConfiguration), typeof(MeteringConfiguration), Applicability.Required),
            new MasterDataValue(nameof(MasterData.ReadingOccurrence), typeof(ReadingOccurrence), Applicability.Required),
            new MasterDataValue(nameof(MasterData.NetSettlementGroup), typeof(NetSettlementGroup), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.PowerLimit), typeof(PowerLimit), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.PowerPlantGsrnNumber), typeof(GsrnNumber), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.DisconnectionType), typeof(DisconnectionType), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.ConnectionType), typeof(ConnectionType), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.EffectiveDate), typeof(EffectiveDate), Applicability.Required),
            new MasterDataValue(nameof(MasterData.Capacity), typeof(Capacity), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.Address), typeof(Address), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.ProductionObligation), typeof(bool), Applicability.Optional),
        };

        public MasterDataUpdater(IEnumerable<MasterDataField> fields, MasterData currentMasterData)
            :base(fields)
        {
            SetValue(nameof(MasterData.ProductType), currentMasterData.ProductType);
        }

        public BusinessRulesValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public MasterData Build()
        {
            if (string.IsNullOrEmpty(_productType) == false)
            {
                SetValue(nameof(MasterData.ProductType), EnumerationType.FromName<ProductType>(_productType));
            }

            var netSettlementGroup = _values.Find(value => value.Name.Equals(nameof(MasterData.NetSettlementGroup))).Value as NetSettlementGroup;
            if (netSettlementGroup != NetSettlementGroup.Six)
            {
                 SetValue<ScheduledMeterReadingDate>(nameof(MasterData.ScheduledMeterReadingDate), null);
            }

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
                nameof(MasterData.SettlementMethod),
                () =>
                {
                    return EnumerationType.GetAll<NetSettlementGroup>()
                        .Select(item => item.Name)
                        .Contains(netSettlementGroup) == false ? BusinessRulesValidationResult.Failure(new InvalidSettlementGroupValue(netSettlementGroup)) : BusinessRulesValidationResult.Valid();
                },
                () => EnumerationType.FromName<NetSettlementGroup>(netSettlementGroup));
            return this;
        }

        public IMasterDataBuilder WithMeteringConfiguration(string method, string? meterNumber)
        {
            throw new NotImplementedException();
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
            _productType = productType;
            return this;
        }

        public IMasterDataBuilder WithConnectionType(string? connectionType)
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder WithProductionObligation(bool? productionObligation)
        {
            throw new NotImplementedException();
        }

        private void SetValueIfValid<T>(string valueName, Func<BusinessRulesValidationResult> validator, Func<T> creator)
        {
            var valueItem = _values.Find(value => value.Name.Equals(valueName));
            valueItem.SetValue(validator, creator);
        }
    }
}
