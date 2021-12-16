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
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling
{
    public class MasterDataBuilder : MasterDataBuilderBase, IMasterDataBuilder
    {
        private readonly List<ValidationError> _validationErrors = new();

        public MasterDataBuilder(IEnumerable<MasterDataField> masterDataFields)
            : base(masterDataFields)
        {
        }

        public IMasterDataBuilder WithNetSettlementGroup(string? netSettlementGroup)
        {
            SetValue(nameof(MasterData.NetSettlementGroup), string.IsNullOrEmpty(netSettlementGroup) ? null : EnumerationType.FromName<NetSettlementGroup>(netSettlementGroup));
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
                netSettlementGroup: GetValue<NetSettlementGroup>(nameof(MasterData.NetSettlementGroup)),
                productionObligation: GetValue<bool?>(nameof(MasterData.ProductionObligation)));
        }

        public IMasterDataBuilder WithMeteringConfiguration(string method, string? meterNumber)
        {
            var meter = string.IsNullOrEmpty(meterNumber) ? MeterId.Empty() : MeterId.Create(meterNumber);
            var meteringMethod = EnumerationType.FromName<MeteringMethod>(method);
            SetValueIfValid(
                nameof(MasterData.MeteringConfiguration),
                () => MeteringConfiguration.CheckRules(meteringMethod, meter),
                () => MeteringConfiguration.Create(meteringMethod, meter));

            return this;
        }

        public IMasterDataBuilder WithAddress(
            string? streetName = null,
            string? streetCode = null,
            string? buildingNumber = null,
            string? city = null,
            string? citySubDivision = null,
            string? postCode = null,
            CountryCode? countryCode = null,
            string? floor = null,
            string? room = null,
            int? municipalityCode = null,
            bool? isActual = null,
            Guid? geoInfoReference = null,
            string? locationDescription = null)
        {
            SetValueIfValid(
                nameof(MasterData.Address),
                () => Address.CheckRules(streetName, streetCode, buildingNumber, city, citySubDivision, postCode, countryCode, floor, room, municipalityCode, locationDescription),
                () => Address.Create(streetName, streetCode, buildingNumber, city, citySubDivision, postCode, countryCode, floor, room, municipalityCode, isActual, geoInfoReference, locationDescription));
            return this;
        }

        public IMasterDataBuilder WithMeasurementUnitType(string? measurementUnitType)
        {
            SetValue(nameof(MasterData.UnitType), measurementUnitType is null ? null : EnumerationType.FromName<MeasurementUnitType>(measurementUnitType!));
            return this;
        }

        public IMasterDataBuilder WithPowerPlant(string? gsrnNumber)
        {
            SetValue(nameof(MasterData.PowerPlantGsrnNumber), string.IsNullOrEmpty(gsrnNumber) ? null : GsrnNumber.Create(gsrnNumber));
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

        public IMasterDataBuilder WithSettlementMethod(string? settlementMethod)
        {
            SetValue(nameof(MasterData.SettlementMethod), string.IsNullOrEmpty(settlementMethod) ? null : EnumerationType.FromName<SettlementMethod>(settlementMethod));
            return this;
        }

        public IMasterDataBuilder WithDisconnectionType(string? disconnectionType)
        {
            SetValue(nameof(MasterData.DisconnectionType), disconnectionType is null ? null : EnumerationType.FromName<DisconnectionType>(disconnectionType));
            return this;
        }

        public IMasterDataBuilder WithAssetType(string? assetType)
        {
            SetValue(nameof(MasterData.AssetType), string.IsNullOrEmpty(assetType) ? null : EnumerationType.FromName<AssetType>(assetType));
            return this;
        }

        public IMasterDataBuilder WithScheduledMeterReadingDate(string? scheduledMeterReadingDate)
        {
            SetValue(nameof(MasterData.ScheduledMeterReadingDate), string.IsNullOrEmpty(scheduledMeterReadingDate) ? null : ScheduledMeterReadingDate.Create(scheduledMeterReadingDate));
            return this;
        }

        public IMasterDataBuilder WithCapacity(double? capacity)
        {
            SetValue(nameof(MasterData.Capacity), capacity is null ? null : Capacity.Create(capacity.Value));
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

        public IMasterDataBuilder WithConnectionType(string? connectionType)
        {
            SetValue(nameof(MasterData.ConnectionType), string.IsNullOrEmpty(connectionType) ? null : EnumerationType.FromName<ConnectionType>(connectionType));
            return this;
        }

        public IMasterDataBuilder WithProductionObligation(bool? productionObligation)
        {
            SetValue(nameof(MasterData.ProductionObligation), productionObligation);
            return this;
        }

        public BusinessRulesValidationResult Validate()
        {
            if (GetMasterValueItem<ReadingOccurrence>(nameof(MasterData.ReadingOccurrence)).HasRequiredValue() == false) _validationErrors.Add(new MeterReadingPeriodicityIsRequired());
            if (GetMasterValueItem<MeasurementUnitType>(nameof(MasterData.UnitType)).HasRequiredValue() == false) _validationErrors.Add(new UnitTypeIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<NetSettlementGroup>(nameof(MasterData.NetSettlementGroup), new NetSettlementGroupIsRequired());
            return new BusinessRulesValidationResult(_validationErrors);
        }

        private void AddValidationErrorIfRequiredFieldIsMissing<T>(string valueName, ValidationError validationError)
        {
            if (GetMasterValueItem<T>(valueName).HasRequiredValue() == false) _validationErrors.Add(validationError);
        }

        private void SetValueIfValid<T>(string valueName, Func<BusinessRulesValidationResult> validator, Func<T> creator)
        {
            if (!GetMasterValueItem<MeteringConfiguration>(valueName).CanBeChanged) return;
            var validationResult = validator.Invoke();
            if (validationResult.Success)
            {
                SetValue(valueName, creator.Invoke());
            }
            else
            {
                _validationErrors.AddRange(validationResult.Errors);
            }
        }
    }
}
