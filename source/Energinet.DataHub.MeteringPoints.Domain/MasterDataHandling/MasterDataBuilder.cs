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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
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

        public IMasterDataBuilder WithNetSettlementGroup(string? netSettlementGroup)
        {
            SetValue(nameof(MasterData.NetSettlementGroup), string.IsNullOrEmpty(netSettlementGroup) ? null : EnumerationType.FromName<NetSettlementGroup>(netSettlementGroup));
            return this;
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
            string? geoInfoReference = null,
            string? locationDescription = null)
        {
            SetValueIfValid(
                nameof(MasterData.Address),
                () => Address.CheckRules(streetName, streetCode, buildingNumber, city, citySubDivision, postCode, countryCode, floor, room, municipalityCode, locationDescription, geoInfoReference),
                () => Address.Create(streetName, streetCode, buildingNumber, city, citySubDivision, postCode, countryCode, floor, room, municipalityCode, isActual, geoInfoReference, locationDescription));
            return this;
        }

        public IMasterDataBuilder WithMeasurementUnitType(string? measurementUnitType)
        {
            if (measurementUnitType?.Length > 0)
            {
                SetValueIfValid(
                    nameof(MasterData.UnitType),
                    () =>
                    {
                        return EnumerationType.GetAll<MeasurementUnitType>()
                            .Select(item => item.Name)
                            .Contains(measurementUnitType) == false ? BusinessRulesValidationResult.Failure(new InvalidUnitTypeValue(measurementUnitType)) : BusinessRulesValidationResult.Valid();
                    },
                    () => EnumerationType.FromName<MeasurementUnitType>(measurementUnitType));
            }

            return this;
        }

        public IMasterDataBuilder WithPowerPlant(string? gsrnNumber)
        {
            SetValue(nameof(MasterData.PowerPlantGsrnNumber), string.IsNullOrEmpty(gsrnNumber) ? null : GsrnNumber.Create(gsrnNumber));
            return this;
        }

        public IMasterDataBuilder WithReadingPeriodicity(string? readingPeriodicity)
        {
            if (readingPeriodicity?.Length == 0) SetValue<ReadingOccurrence>(nameof(MasterData.ReadingOccurrence), null);
            if (readingPeriodicity?.Length > 0)
            {
                SetValueIfValid(
                    nameof(MasterData.ReadingOccurrence),
                    () =>
                    {
                        return EnumerationType.GetAll<ReadingOccurrence>()
                            .Select(item => item.Name)
                            .Contains(readingPeriodicity) == false ? BusinessRulesValidationResult
                            .Failure(new InvalidReadingPeriodicityType(readingPeriodicity)) : BusinessRulesValidationResult.Valid();
                    },
                    () => EnumerationType.FromName<ReadingOccurrence>(readingPeriodicity!));
            }

            return this;
        }

        public IMasterDataBuilder WithPowerLimit(int? kwh, int? ampere)
        {
            SetValue(
                nameof(MasterData.PowerLimit),
                PowerLimit.Create(kwh.GetValueOrDefault(), ampere.GetValueOrDefault()));
            return this;
        }

        public IMasterDataBuilder WithPowerLimit(string? kwh, string? ampere)
        {
            SetValueIfValid(
                nameof(MasterData.PowerLimit),
                () => PowerLimit.CheckRules(kwh, ampere),
                () => PowerLimit.Create(kwh, ampere));
            return this;
        }

        public IMasterDataBuilder WithSettlementMethod(string? settlementMethod)
        {
            if (string.IsNullOrEmpty(settlementMethod))
            {
                SetValue<SettlementMethod>(nameof(MasterData.SettlementMethod), null);
            }
            else
            {
                SetValueIfValid(
                    nameof(MasterData.SettlementMethod),
                    () =>
                    {
                        if (EnumerationType.GetAll<SettlementMethod>()
                            .Select(item => item.Name)
                            .Contains(settlementMethod) == false)
                        {
                            return BusinessRulesValidationResult.Failure(new InvalidSettlementMethodValue(settlementMethod));
                        }

                        return BusinessRulesValidationResult.Valid();
                    },
                    () => EnumerationType.FromName<SettlementMethod>(settlementMethod));
            }

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

        public IMasterDataBuilder WithCapacity(string? capacity)
        {
            SetValue(nameof(MasterData.Capacity), capacity is null ? null : Capacity.Create(capacity));
            return this;
        }

        public IMasterDataBuilder EffectiveOn(string? effectiveDate)
        {
            SetValue(nameof(MasterData.EffectiveDate), effectiveDate is null ? null : EffectiveDate.Create(effectiveDate));
            return this;
        }

        public IMasterDataBuilder WithProductType(string? productType)
        {
            if (productType?.Length > 0)
            {
                SetValueIfValid(
                    nameof(MasterData.ProductType),
                    () =>
                    {
                        return EnumerationType.GetAll<ProductType>()
                            .Select(item => item.Name)
                            .Contains(productType) == false ? BusinessRulesValidationResult.Failure(new InvalidProductTypeValue(productType)) : BusinessRulesValidationResult.Valid();
                    },
                    () => EnumerationType.FromName<ProductType>(productType));
            }

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
            _validationErrors.Clear();
            _validationErrors.AddRange(AllValueValidationErrors());

            AddValidationErrorIfRequiredFieldIsMissing<ProductType>(nameof(MasterData.ProductType), new ProductTypeIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<ReadingOccurrence>(nameof(MasterData.ReadingOccurrence), new MeterReadingPeriodicityIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<MeasurementUnitType>(nameof(MasterData.UnitType), new UnitTypeIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<NetSettlementGroup>(nameof(MasterData.NetSettlementGroup), new NetSettlementGroupIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<SettlementMethod>(nameof(MasterData.SettlementMethod), new SettlementMethodIsRequired());

            return new BusinessRulesValidationResult(_validationErrors);
        }

        private void AddValidationErrorIfRequiredFieldIsMissing<T>(string valueName, ValidationError validationError)
        {
            var valueItem = GetMasterValueItem<T>(valueName);
            if (valueItem.HasErrors())
            {
                return;
            }

            if (valueItem.HasRequiredValue() == false) _validationErrors.Add(validationError);
        }

        private void SetValueIfValid<T>(string valueName, Func<BusinessRulesValidationResult> validator, Func<T> creator)
        {
            var valueItem = GetMasterValueItem<T>(valueName);
            valueItem.SetValue(validator, creator);
        }
    }
}
