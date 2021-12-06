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
using System.Diagnostics.PerformanceData;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    #pragma warning disable
    public interface IMasterDataBuilder
    {
        BusinessRulesValidationResult Validate();
        IMasterDataBuilder WithNetSettlementGroup(string netSettlementGroup);
        MasterData Build();
        IMasterDataBuilder WithMeteringConfiguration(string method, string meterNumber);
        IMasterDataBuilder WithAddress(string? streetName = null, string? streetCode = null, string? buildingNumber = null, string? city = null, string? citySubDivision = null, string? postCode = null, CountryCode? countryCode = null, string? floor = null, string? room = null, int? municipalityCode = null, bool? isActual = null, Guid? geoInfoReference = null, string? locationDescription = null);
        IMasterDataBuilder WithMeasurementUnitType(string? measurementUnitType);
        IMasterDataBuilder WithPowerPlant(string gsrnNumber);
        IMasterDataBuilder WithReadingPeriodicity(string? readingPeriodicity);
        IMasterDataBuilder WithPowerLimit(int kwh, int ampere);
        IMasterDataBuilder WithSettlementMethod(string settlementMethod);
        IMasterDataBuilder WithDisconnectionType(string disconnectionType);
        IMasterDataBuilder WithAssetType(string? assetType);
        IMasterDataBuilder WithScheduledMeterReadingDate(string scheduledMeterReadingDate);
        IMasterDataBuilder WithCapacity(double? capacity);
        IMasterDataBuilder EffectiveOn(string? effectiveDate);
        IMasterDataBuilder WithProductType(string productType);
        IMasterDataBuilder WithConnectionType(string? connectionType);
    }

    public class TestMasterDataBuilder : IMasterDataBuilder
    {
        private NetSettlementGroup _netSettlementGroup = NetSettlementGroup.Zero;
        private MeteringConfiguration _meteringConfiguration = MeteringConfiguration.Create(MeteringMethod.Virtual, MeterId.Empty());
        private Address _address = Address.Create("Test street", countryCode: CountryCode.DK);
        private MeasurementUnitType _measurementUnitType = MeasurementUnitType.KWh;
        private GsrnNumber? _powerPlanGsrnNumber;
        private ReadingOccurrence? _readingPeriodicity = ReadingOccurrence.Hourly;
        private PowerLimit? _powerLimit;
        private SettlementMethod? _settlementMethod;
        private DisconnectionType _disconnectionType = DisconnectionType.Manual;
        private AssetType? _assetType;
        private ScheduledMeterReadingDate? _scheduledMeterReadingDate;
        private Capacity? _capacity;
        private EffectiveDate _effectiveDate = EffectiveDate.Create(SampleData.EffectiveDate);
        private ProductType _productType = ProductType.EnergyActive;
        private ConnectionType? _connectionType;

        public BusinessRulesValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder WithNetSettlementGroup(string netSettlementGroup)
        {
            _netSettlementGroup = EnumerationType.FromName<NetSettlementGroup>(netSettlementGroup);
            return this;
        }

        public MasterData Build()
        {
            return new MasterData(
                productType: _productType!,
                unitType: _measurementUnitType!,
                assetType: _assetType!,
                readingOccurrence: _readingPeriodicity!,
                powerLimit: _powerLimit!,
                powerPlantGsrnNumber: _powerPlanGsrnNumber!,
                effectiveDate: _effectiveDate!,
                capacity: _capacity,
                address: _address!,
                meteringConfiguration: _meteringConfiguration!,
                settlementMethod: _settlementMethod!,
                scheduledMeterReadingDate: _scheduledMeterReadingDate,
                connectionType: _connectionType,
                disconnectionType: _disconnectionType!,
                netSettlementGroup: _netSettlementGroup!);
        }

        public IMasterDataBuilder WithMeteringConfiguration(string method, string meterNumber)
        {
            _meteringConfiguration = MeteringConfiguration.Create(EnumerationType.FromName<MeteringMethod>(method), MeterId.Create(meterNumber));
            return this;
        }

        public IMasterDataBuilder WithAddress(string? streetName = null, string? streetCode = null, string? buildingNumber = null, string? city = null, string? citySubDivision = null, string? postCode = null, CountryCode? countryCode = null, string? floor = null, string? room = null, int? municipalityCode = null, bool? isActual = null, Guid? geoInfoReference = null, string? locationDescription = null)
        {
            _address = Address.Create(
                streetName,
                streetCode,
                buildingNumber,
                city,
                citySubDivision,
                postCode,
                countryCode,
                floor,
                room,
                municipalityCode,
                isActual,
                geoInfoReference,
                locationDescription);
            return this;
        }

        public IMasterDataBuilder WithMeasurementUnitType(string measurementUnitType)
        {
            _measurementUnitType = EnumerationType.FromName<MeasurementUnitType>(measurementUnitType);
            return this;
        }

        public IMasterDataBuilder WithPowerPlant(string? gsrnNumber)
        {
            _powerPlanGsrnNumber = gsrnNumber is null ? null : GsrnNumber.Create(gsrnNumber);
            return this;
        }

        public IMasterDataBuilder WithReadingPeriodicity(string? readingPeriodicity)
        {
            _readingPeriodicity = EnumerationType.FromName<ReadingOccurrence>(readingPeriodicity);
            return this;
        }

        public IMasterDataBuilder WithPowerLimit(int kwh, int ampere)
        {
            _powerLimit = PowerLimit.Create(kwh, ampere);
            return this;
        }

        public IMasterDataBuilder WithSettlementMethod(string settlementMethod)
        {
            _settlementMethod = EnumerationType.FromName<SettlementMethod>(settlementMethod);
            return this;
        }

        public IMasterDataBuilder WithDisconnectionType(string disconnectionType)
        {
            _disconnectionType = EnumerationType.FromName<DisconnectionType>(disconnectionType);
            return this;
        }

        public IMasterDataBuilder WithAssetType(string? assetType)
        {
            _assetType = assetType is null ? null : EnumerationType.FromName<AssetType>(assetType);
            return this;
        }

        public IMasterDataBuilder WithScheduledMeterReadingDate(string scheduledMeterReadingDate)
        {
            _scheduledMeterReadingDate = ScheduledMeterReadingDate.Create(scheduledMeterReadingDate);
            return this;
        }

        public IMasterDataBuilder WithCapacity(double? capacityInKwh)
        {
            _capacity = capacityInKwh is null ? null : Capacity.Create(capacityInKwh.Value);
            return this;
        }

        public IMasterDataBuilder EffectiveOn(string? effectiveDate)
        {
            _effectiveDate = effectiveDate is null ? null : EffectiveDate.Create(effectiveDate);
            return this;
        }

        public IMasterDataBuilder WithProductType(string productType)
        {
            _productType = EnumerationType.FromName<ProductType>(productType);
            return this;
        }

        public IMasterDataBuilder WithConnectionType(string? connectionType)
        {
            _connectionType = connectionType is null ? null : EnumerationType.FromName<ConnectionType>(connectionType);
            return this;
        }
    }
}
