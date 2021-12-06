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
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class ConsumptionBuilderTests
    {
        [Fact]
        public void Product_type_is_set_to_active_energy()
        {
            var fields = new MasterDataFieldSelector();
            var sut = new MasterDataBuilder(fields.GetMasterDataFieldsFor(MeteringPointType.Consumption))
                .WithProductType(ProductType.PowerReactive.Name)
                .Build();

            Assert.Equal(ProductType.EnergyActive, sut.ProductType);
        }

        [Fact]
        public void Measurement_unit_type_is_Kwh()
        {
            var fields = new MasterDataFieldSelector();
            var sut = new MasterDataBuilder(fields.GetMasterDataFieldsFor(MeteringPointType.Consumption))
                .WithMeasurementUnitType(MeasurementUnitType.Tonne.Name)
                .Build();

            Assert.Equal(MeasurementUnitType.KWh, sut.UnitType);
        }

        [Fact]
        public void Required_fields_have_values()
        {
            var fields = new MasterDataFieldSelector();
            var masterData = new MasterDataBuilder(fields.GetMasterDataFieldsFor(MeteringPointType.Consumption))
                .WithAddress("Test Street")
                .WithAssetType(AssetType.Boiler.Name)
                .WithSettlementMethod(SettlementMethod.Profiled.Name)
                .WithScheduledMeterReadingDate("0101")
                .WithMeteringConfiguration(MeteringMethod.Physical.Name, "1234")
                .WithProductType(ProductType.PowerActive.Name)
                .WithReadingPeriodicity(ReadingOccurrence.Yearly.Name)
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithPowerLimit(0, 0)
                .WithPowerPlant(SampleData.PowerPlant)
                .WithDisconnectionType(DisconnectionType.Manual.Name)
                .WithConnectionType(ConnectionType.Direct.Name)
                .EffectiveOn(SampleData.EffectiveDate)
                .WithCapacity(1)
                .Build();

            Assert.NotNull(masterData.ProductType);
            Assert.NotNull(masterData.AssetType);
            Assert.NotNull(masterData.SettlementMethod);
            Assert.NotNull(masterData.ScheduledMeterReadingDate);
            Assert.NotNull(masterData.MeteringConfiguration);
            Assert.NotNull(masterData.ReadingOccurrence);
            Assert.NotNull(masterData.NetSettlementGroup);
            Assert.NotNull(masterData.PowerLimit);
            Assert.NotNull(masterData.PowerPlantGsrnNumber);
            Assert.NotNull(masterData.DisconnectionType);
            Assert.NotNull(masterData.ConnectionType);
            Assert.NotNull(masterData.EffectiveDate);
            Assert.NotNull(masterData.Capacity);
            Assert.NotNull(masterData.Address);
        }
    }

    #pragma warning disable

    internal class MasterDataFieldSelector
    {
        private readonly Dictionary<string, List<MasterDataField>> _fields = new()
        {
            {
                MeteringPointType.Consumption.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required, false, ProductType.EnergyActive),
                    new(nameof(MasterData.UnitType), Applicability.Required, false, MeasurementUnitType.KWh),
                    new(nameof(MasterData.AssetType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.Optional),
                    new(nameof(MasterData.ScheduledMeterReadingDate), Applicability.Optional),
                    new(nameof(MasterData.ReadingOccurrence), Applicability.Required),
                    new(nameof(MasterData.NetSettlementGroup), Applicability.Required),
                    new(nameof(MasterData.DisconnectionType), Applicability.Required),
                    new(nameof(MasterData.ConnectionType), Applicability.Optional),
                }
            },
            {
                MeteringPointType.Production.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ScheduledMeterReadingDate), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductType), Applicability.Required, false, ProductType.EnergyActive),
                    new(nameof(MasterData.UnitType), Applicability.Required, true, MeasurementUnitType.KWh),
                }
            }

        };

        public IEnumerable<MasterDataField> GetMasterDataFieldsFor(MeteringPointType meteringPointType)
        {
            if (_fields.ContainsKey(meteringPointType.Name))
            {
                return _fields[meteringPointType.Name];
            }

            return new List<MasterDataField>().AsReadOnly();
        }
    }

    public enum Applicability
    {
        Required,
        Optional,
        NotAllowed,
    }
    internal class MasterDataValue
    {
        public MasterDataValue(string name, Type valueType, Applicability applicability)
        {
            Name = name;
            ValueType = valueType;
            Applicability = applicability;
        }

        public string Name { get; }
        public Type ValueType { get; }
        public Applicability Applicability { get; private set; }
        public object? Value { get; private set; }
        public bool CanBeChanged { get; private set; } = true;

        public void SetValue<T>(T value)
        {
            Value = value;
        }

        public void SetApplicability(Applicability applicability)
        {
            Applicability = applicability;
        }

        public void CanNotBeChanged()
        {
            CanBeChanged = false;
        }

        public bool HasRequiredValue()
        {
            return !(Applicability == Applicability.Required && Value is null);
        }
    }

    public class MasterDataField
    {
        public MasterDataField(string name, Applicability applicability, bool canBeChanged = true, object? defaultValue = null)
        {
            Name = name;
            Applicability = applicability;
            CanBeChanged = canBeChanged;
            DefaultValue = defaultValue;
        }

        public string Name { get; }
        public Applicability Applicability { get; }
        public bool CanBeChanged { get; }
        public object? DefaultValue { get; }
    }

    internal abstract class MasterDataBuilderBase
    {
        private readonly List<MasterDataValue> _values = new()
        {
            new MasterDataValue(nameof(MasterData.ProductType), typeof(ProductType), Applicability.Required),
            new MasterDataValue(nameof(MasterData.UnitType), typeof(MeasurementUnitType), Applicability.Required),
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
        };

        protected MasterDataBuilderBase(IEnumerable<MasterDataField> masterDataFields)
        {
            masterDataFields.ToList().ForEach(field => ConfigureValue(field.Name, field.Applicability, field.DefaultValue, field.CanBeChanged));
        }

        protected void SetValue<T>(string name, T value)
        {
            var valueItem = GetMasterValueItem<T>(name);
            if (valueItem.CanBeChanged && valueItem.Applicability != Applicability.NotAllowed)
            {
                valueItem.SetValue(value);
            }
        }

        protected T GetValue<T>(string name)
        {
            return (T)GetMasterValueItem<T>(name).Value;
        }

        protected MasterDataValue GetMasterValueItem<T>(string name)
        {
            return _values.First(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private void ConfigureValue<T>(string name, Applicability applicability, T? defaultValue = default(T), bool canBeChanged = true)
        {
            var value = GetMasterValueItem<T>(name);

            if (canBeChanged == false)
            {
                value.CanNotBeChanged();
            }

            value.SetValue(defaultValue);
            value.SetApplicability(applicability);
        }
    }
    internal class MasterDataBuilder : MasterDataBuilderBase, IMasterDataBuilder
    {
        public MasterDataBuilder(IEnumerable<MasterDataField> masterDataFields)
            : base(masterDataFields)
        {
        }

        public IMasterDataBuilder WithNetSettlementGroup(string? netSettlementGroup)
        {
            SetValue(nameof(MasterData.NetSettlementGroup), netSettlementGroup is null ? null : EnumerationType.FromName<NetSettlementGroup>(netSettlementGroup));
            return this;
        }

        public MasterData Build()
        {
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
                netSettlementGroup: GetValue<NetSettlementGroup>(nameof(MasterData.NetSettlementGroup))
            );
        }

        public IMasterDataBuilder WithMeteringConfiguration(string method, string meterNumber)
        {
            SetValue(nameof(MasterData.MeteringConfiguration), MeteringConfiguration.Create(EnumerationType.FromName<MeteringMethod>(method), MeterId.Create(meterNumber)));
            return this;
        }

        public IMasterDataBuilder WithAddress(string? streetName = null, string? streetCode = null, string? buildingNumber = null,
            string? city = null, string? citySubDivision = null, string? postCode = null, CountryCode? countryCode = null,
            string? floor = null, string? room = null, int? municipalityCode = null, bool? isActual = null,
            Guid? geoInfoReference = null, string? locationDescription = null)
        {
            SetValue(nameof(MasterData.Address), Address.Create(streetName, streetCode, buildingNumber, city, citySubDivision, postCode, countryCode, floor, room, municipalityCode, isActual, geoInfoReference, locationDescription));
            return this;
        }

        public IMasterDataBuilder WithMeasurementUnitType(string? measurementUnitType)
        {
            SetValue(nameof(MasterData.UnitType), measurementUnitType is null ? null : EnumerationType.FromName<MeasurementUnitType>(measurementUnitType!));
            return this;
        }

        public IMasterDataBuilder WithPowerPlant(string? gsrnNumber)
        {
            SetValue(nameof(MasterData.PowerPlantGsrnNumber), gsrnNumber is null ? null : GsrnNumber.Create(gsrnNumber));
            return this;
        }

        public IMasterDataBuilder WithReadingPeriodicity(string? readingPeriodicity)
        {
            SetValue(nameof(MasterData.ReadingOccurrence),  readingPeriodicity is null ? null : EnumerationType.FromName<ReadingOccurrence>(readingPeriodicity));
            return this;
        }

        public IMasterDataBuilder WithPowerLimit(int kwh, int ampere)
        {
            SetValue(nameof(MasterData.PowerLimit), PowerLimit.Create(kwh, ampere));
            return this;
        }

        public IMasterDataBuilder WithSettlementMethod(string settlementMethod)
        {
            SetValue(nameof(MasterData.SettlementMethod), EnumerationType.FromName<SettlementMethod>(settlementMethod));
            return this;
        }

        public IMasterDataBuilder WithDisconnectionType(string? disconnectionType)
        {
            SetValue(nameof(MasterData.DisconnectionType), disconnectionType is null ? null : EnumerationType.FromName<DisconnectionType>(disconnectionType));
            return this;
        }

        public IMasterDataBuilder WithAssetType(string? assetType)
        {
            SetValue(nameof(MasterData.AssetType), assetType is null ? null : EnumerationType.FromName<AssetType>(assetType));
            return this;
        }

        public IMasterDataBuilder WithScheduledMeterReadingDate(string scheduledMeterReadingDate)
        {
            SetValue(nameof(MasterData.ScheduledMeterReadingDate), ScheduledMeterReadingDate.Create(scheduledMeterReadingDate));
            return this;
        }

        public IMasterDataBuilder WithCapacity(double? capacityInKwh)
        {
            SetValue(nameof(MasterData.Capacity), capacityInKwh is null ? null : Capacity.Create(capacityInKwh.Value));
            return this;
        }

        public IMasterDataBuilder EffectiveOn(string? effectiveDate)
        {
            SetValue(nameof(MasterData.EffectiveDate), effectiveDate is null ? null : EffectiveDate.Create(effectiveDate));
            return this;
        }

        public IMasterDataBuilder WithProductType(string productType)
        {
            SetValue(nameof(MasterData.ProductType), EnumerationType.FromName<ProductType>(productType));
            return this;
        }

        public IMasterDataBuilder WithConnectionType(string connectionType)
        {
            SetValue(nameof(MasterData.ConnectionType), EnumerationType.FromName<ConnectionType>(connectionType));
            return this;
        }

        public BusinessRulesValidationResult Validate()
        {
            var validationErrors = new List<ValidationError>();
            if (GetMasterValueItem<ReadingOccurrence>(nameof(MasterData.ReadingOccurrence)).HasRequiredValue() == false) validationErrors.Add(new MeterReadingPeriodicityIsRequired());
            if (GetMasterValueItem<MeasurementUnitType>(nameof(MasterData.UnitType)).HasRequiredValue() == false) validationErrors.Add(new UnitTypeIsRequired());

            return new BusinessRulesValidationResult(validationErrors);
        }
    }
}

