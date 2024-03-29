﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.MeteringPoints.Domain.Actors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Exceptions;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling
{
    public class MasterDataUpdater : MasterDataBuilderBase
    {
        private readonly MasterData _currentMasterData;
        private readonly List<ValidationError> _validationErrors = new();

        public MasterDataUpdater(IEnumerable<MasterDataField> fields, MasterData currentMasterData)
            : base(fields)
        {
            if (currentMasterData == null) throw new ArgumentNullException(nameof(currentMasterData));
            _currentMasterData = currentMasterData;
            PopulateValuesFrom(_currentMasterData);
        }

        public BusinessRulesValidationResult Validate()
        {
            _validationErrors.Clear();
            _validationErrors.AddRange(AllValueValidationErrors());

            AddValidationErrorIfRequiredFieldIsMissing<MeasurementUnitType>(nameof(MasterData.UnitType), new UnitTypeIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<GsrnNumber>(nameof(MasterData.PowerPlantGsrnNumber), new PowerPlantIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<ReadingOccurrence>(nameof(MasterData.ReadingOccurrence), new MeterReadingPeriodicityIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<SettlementMethod>(nameof(MasterData.SettlementMethod), new SettlementMethodIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<DisconnectionType>(nameof(MasterData.DisconnectionType), new DisconnectionTypeIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<ConnectionType>(nameof(MasterData.ConnectionType), new ConnectionTypeIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<AssetType>(nameof(MasterData.AssetType), new AssetTypeIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<ScheduledMeterReadingDate>(nameof(MasterData.ScheduledMeterReadingDate), new ScheduledMeterReadingDateIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<Capacity>(nameof(MasterData.Capacity), new CapacityIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<EffectiveDate>(nameof(MasterData.EffectiveDate), new EffectiveDateIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<NetSettlementGroup>(nameof(MasterData.NetSettlementGroup), new NetSettlementGroupIsRequired());
            AddValidationErrorIfRequiredFieldIsMissing<ProductType>(nameof(MasterData.ProductType), new ProductTypeIsRequired());

            return new BusinessRulesValidationResult(_validationErrors);
        }

        public MasterData Build()
        {
            ThrowIfAnyValidationErrors();
            RemoveConflictingValues();
            return CreateMasterData();
        }

        public MasterDataUpdater WithNetSettlementGroup(string? netSettlementGroup)
        {
            if (netSettlementGroup?.Length == 0) SetValue<NetSettlementGroup>(nameof(MasterData.NetSettlementGroup), null);
            if (netSettlementGroup?.Length > 0)
            {
                SetValueIfValid(
                    nameof(MasterData.NetSettlementGroup),
                    () =>
                    {
                        return EnumerationType.GetAll<NetSettlementGroup>()
                            .Select(item => item.Name)
                            .Contains(netSettlementGroup) == false ? BusinessRulesValidationResult.Failure(new InvalidNetSettlementGroupValue()) : BusinessRulesValidationResult.Valid();
                    },
                    () => EnumerationType.FromName<NetSettlementGroup>(netSettlementGroup));
            }

            return this;
        }

        public MasterDataUpdater WithMeteringConfiguration(string? method, string? meterNumber)
        {
            var currentMeterConfiguration = GetValue<MeteringConfiguration>(nameof(MasterData.MeteringConfiguration));
            SetValueIfValid(
                nameof(MasterData.MeteringConfiguration),
                () =>
                {
                    if (method?.Length == 0)
                    {
                        return BusinessRulesValidationResult.Failure(new MeteringMethodIsRequired());
                    }

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

                    var updatedMeteringMethod = method is null
                        ? currentMeterConfiguration.Method
                        : EnumerationType.FromName<MeteringMethod>(method);

                    var updatedMeterNumber = GetUpdatedMeterNumber(meterNumber, updatedMeteringMethod, currentMeterConfiguration);

                    var validationResult = MeteringConfiguration.CheckRules(updatedMeteringMethod, updatedMeterNumber);
                    return validationResult.Success == false ? BusinessRulesValidationResult.Failure(validationResult.Errors.ToArray()) : BusinessRulesValidationResult.Valid();
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

        public MasterDataUpdater WithAddress(string? streetName = null, string? streetCode = null, string? buildingNumber = null, string? city = null, string? citySubDivision = null, string? postCode = null, string? countryCode = null, string? floor = null, string? room = null, int? municipalityCode = null, bool? isActual = null, string? geoInfoReference = null, string? locationDescription = null)
        {
            SetValueIfValid(
                nameof(MasterData.Address),
                () =>
                {
                    if (countryCode is not null)
                    {
                        if (EnumerationType.GetAll<CountryCode>()
                                .Select(item => item.Name)
                                .Contains(countryCode) == false)
                        {
                            return BusinessRulesValidationResult.Failure(new CountryCodeValidRuleError());
                        }
                    }

                    var currentCountryCode =
                        countryCode == null ? null : EnumerationType.FromName<CountryCode>(countryCode);
                    return Address.CheckRules(streetName, streetCode, buildingNumber, city, citySubDivision, postCode, currentCountryCode, floor, room, municipalityCode, locationDescription, geoInfoReference, isActual);
                },
                () =>
                {
                    var currentCountryCode =
                        countryCode == null ? null : EnumerationType.FromName<CountryCode>(countryCode);
                    var currentAddress = GetValue<Address>(nameof(MasterData.Address));
                    var address = Address.Create(streetName, streetCode, buildingNumber, city, citySubDivision, postCode, currentCountryCode, floor, room, municipalityCode, isActual, geoInfoReference, locationDescription);
                    return currentAddress.MergeFrom(address);
                });

            return this;
        }

        public MasterDataUpdater WithMeasurementUnitType(string? measurementUnitType)
        {
            if (measurementUnitType?.Length == 0) SetValue<MeasurementUnitType>(nameof(MasterData.UnitType), null);
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

        public MasterDataUpdater WithPowerPlant(string? gsrnNumber)
        {
            if (gsrnNumber?.Length == 0) SetValue<GsrnNumber>(nameof(MasterData.PowerPlantGsrnNumber), null);
            if (gsrnNumber?.Length > 0)
            {
                SetValueIfValid(
                nameof(MasterData.PowerPlantGsrnNumber),
                () => GsrnNumber.CheckRules(gsrnNumber),
                () => GsrnNumber.Create(gsrnNumber));
            }

            return this;
        }

        public MasterDataUpdater WithReadingPeriodicity(string? readingPeriodicity)
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

        public MasterDataUpdater WithPowerLimit(int? kwh, int? ampere)
        {
            SetValueIfValid(
                nameof(MasterData.PowerLimit),
                () => PowerLimit.CheckRules(kwh, ampere),
                () => PowerLimit.Create(kwh, ampere));
            return this;
        }

        public MasterDataUpdater WithPowerLimit(string? kwh, string? ampere)
        {
            var updatedKwh = _currentMasterData.PowerLimit is null
                ? ConvertToNullableString(kwh, null)
                : ConvertToNullableString(kwh, GetValue<PowerLimit>(nameof(MasterData.PowerLimit)).Kwh);
            var updatedAmpere = _currentMasterData.PowerLimit is null
                ? ConvertToNullableString(ampere, null)
                : ConvertToNullableString(ampere, GetValue<PowerLimit>(nameof(MasterData.PowerLimit)).Ampere);

            SetValueIfValid(
                nameof(MasterData.PowerLimit),
                () => PowerLimit.CheckRules(updatedKwh, updatedAmpere),
                () => PowerLimit.Create(updatedKwh, updatedAmpere));

            return this;
        }

        public MasterDataUpdater WithSettlementMethod(string? settlementMethod)
        {
            if (settlementMethod?.Length == 0) SetValue<SettlementMethod>(nameof(MasterData.SettlementMethod), null);
            if (settlementMethod?.Length > 0)
            {
                SetValueIfValid(
                    nameof(MasterData.SettlementMethod),
                    () =>
                    {
                        return EnumerationType.GetAll<SettlementMethod>()
                            .Select(item => item.Name)
                            .Contains(settlementMethod) == false
                            ? BusinessRulesValidationResult.Failure(new InvalidSettlementMethodValue(settlementMethod))
                            : BusinessRulesValidationResult.Valid();
                    },
                    () => EnumerationType.FromName<SettlementMethod>(settlementMethod));
            }

            return this;
        }

        public MasterDataUpdater WithDisconnectionType(string? disconnectionType)
        {
            if (disconnectionType?.Length == 0) SetValue<DisconnectionType>(nameof(MasterData.DisconnectionType), null);
            if (disconnectionType?.Length > 0)
            {
                SetValueIfValid(
                    nameof(MasterData.DisconnectionType),
                    () =>
                    {
                        return EnumerationType
                            .GetAll<DisconnectionType>()
                            .Select(item => item.Name)
                            .Contains(disconnectionType) == false ? BusinessRulesValidationResult.Failure(new InvalidDisconnectionTypeValue(disconnectionType)) : BusinessRulesValidationResult.Valid();
                    },
                    () => EnumerationType.FromName<DisconnectionType>(disconnectionType));
            }

            return this;
        }

        public MasterDataUpdater WithAssetType(string? assetType)
        {
            if (assetType?.Length == 0) SetValue<AssetType>(nameof(MasterData.AssetType), null);
            if (assetType?.Length > 0)
            {
                SetValueIfValid(
                    nameof(MasterData.AssetType),
                    () =>
                    {
                        return EnumerationType.GetAll<AssetType>()
                            .Select(item => item.Name)
                            .Contains(assetType) == false ? BusinessRulesValidationResult.Failure(new InvalidAssetTypeValue(assetType)) : BusinessRulesValidationResult.Valid();
                    },
                    () => EnumerationType.FromName<AssetType>(assetType!));
            }

            return this;
        }

        public MasterDataUpdater WithScheduledMeterReadingDate(string? scheduledMeterReadingDate)
        {
            if (scheduledMeterReadingDate?.Length == 0) SetValue<ScheduledMeterReadingDate>(nameof(MasterData.ScheduledMeterReadingDate), null);
            if (scheduledMeterReadingDate?.Length > 0)
            {
                SetValueIfValid(
                    nameof(MasterData.ScheduledMeterReadingDate),
                    () => ScheduledMeterReadingDate.CheckRules(scheduledMeterReadingDate),
                    () => ScheduledMeterReadingDate.Create(scheduledMeterReadingDate!));
            }

            return this;
        }

        public MasterDataUpdater WithCapacity(string? capacity)
        {
            if (capacity?.Length == 0) SetValue<Capacity>(nameof(MasterData.Capacity), null);
            if (capacity?.Length > 0)
            {
                SetValueIfValid(
                    nameof(MasterData.Capacity),
                    () => Capacity.CheckRules(capacity),
                    () => Capacity.Create(capacity));
            }

            return this;
        }

        public MasterDataUpdater EffectiveOn(string? effectiveDate)
        {
            SetValueIfValid(
                nameof(MasterData.EffectiveDate),
                () => EffectiveDate.CheckRules(effectiveDate!),
                () => EffectiveDate.Create(effectiveDate!));
            return this;
        }

        public MasterDataUpdater WithProductType(string? productType)
        {
            if (productType?.Length == 0) SetValue<ProductType>(nameof(MasterData.ProductType), null);
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

        public MasterDataUpdater WithConnectionType(string? connectionType)
        {
            if (connectionType?.Length == 0) SetValue<ConnectionType>(nameof(MasterData.ConnectionType), null);
            if (connectionType?.Length > 0)
            {
                SetValueIfValid(
                nameof(MasterData.ConnectionType),
                () =>
                {
                    return EnumerationType.GetAll<ConnectionType>()
                        .Select(item => item.Name)
                        .Contains(connectionType) == false ? BusinessRulesValidationResult.Failure(new InvalidConnectionTypeValue(connectionType)) : BusinessRulesValidationResult.Valid();
                },
                () => EnumerationType.FromName<ConnectionType>(connectionType));
            }

            return this;
        }

        public MasterDataUpdater WithProductionObligation(bool? productionObligation)
        {
            if (productionObligation.HasValue == true)
            {
                SetValue(nameof(MasterData.ProductionObligation), productionObligation.GetValueOrDefault());
            }

            return this;
        }

        private static string? ConvertToNullableString(string? updatedValue, int? currentValue)
        {
            if (updatedValue is null) return currentValue.ToString();
            if (updatedValue.Length == 0) return null;
            return updatedValue;
        }

        private static MeterId GetUpdatedMeterNumber(string? meterNumber, MeteringMethod meteringMethod, MeteringConfiguration currentMeterConfiguration)
        {
            if (meteringMethod != MeteringMethod.Physical)
            {
                return MeterId.Empty();
            }

            if (meterNumber?.Length > 0)
            {
                return MeterId.Create(meterNumber);
            }

            return meterNumber is null ? currentMeterConfiguration.Meter : MeterId.Empty();
        }

        private void SetValueIfValid<T>(string valueName, Func<BusinessRulesValidationResult> validator, Func<T> creator)
        {
            var valueItem = GetMasterValueItem<T>(valueName);
            valueItem.SetValue(validator, creator);
        }

        private void RemoveValueIfNotApplicable<T>(string valueName, Func<bool> rule)
        {
            if (rule())
            {
                SetValue<T>(valueName, default(T));
            }
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

        private void ThrowIfAnyValidationErrors()
        {
            var validationResult = Validate();
            if (validationResult.Success == false)
            {
                throw new MasterDataChangeException(validationResult.Errors);
            }
        }

        private void PopulateValuesFrom(MasterData currentMasterData)
        {
            SetValue(nameof(MasterData.ProductType), currentMasterData.ProductType);
            SetValue(nameof(MasterData.NetSettlementGroup), currentMasterData.NetSettlementGroup);
            SetValue(nameof(MasterData.ConnectionType), currentMasterData.ConnectionType);
            SetValue(nameof(MasterData.MeteringConfiguration), currentMasterData.MeteringConfiguration);
            SetValue(nameof(MasterData.Address), currentMasterData.Address);
            SetValue(nameof(MasterData.UnitType), currentMasterData.UnitType);
            SetValue(nameof(MasterData.PowerPlantGsrnNumber), currentMasterData.PowerPlantGsrnNumber);
            SetValue(nameof(MasterData.ReadingOccurrence), currentMasterData.ReadingOccurrence);
            SetValue(nameof(MasterData.SettlementMethod), currentMasterData.SettlementMethod);
            SetValue(nameof(MasterData.DisconnectionType), currentMasterData.DisconnectionType);
            SetValue(nameof(MasterData.AssetType), currentMasterData.AssetType);
            SetValue(nameof(MasterData.ScheduledMeterReadingDate), currentMasterData.ScheduledMeterReadingDate);
            SetValue(nameof(MasterData.Capacity), currentMasterData.Capacity);
            SetValue(nameof(MasterData.ProductionObligation), currentMasterData.ProductionObligation);
            SetValue(nameof(MasterData.PowerLimit), currentMasterData.PowerLimit);
        }

        private void RemoveConflictingValues()
        {
            RemoveValueIfNotApplicable<ScheduledMeterReadingDate>(
                nameof(MasterData.ScheduledMeterReadingDate),
                () => GetValue<NetSettlementGroup>(nameof(MasterData.NetSettlementGroup)) != NetSettlementGroup.Six);
            RemoveValueIfNotApplicable<ConnectionType>(
                nameof(MasterData.ConnectionType),
                () => GetValue<NetSettlementGroup>(nameof(MasterData.NetSettlementGroup)) == NetSettlementGroup.Zero);
        }

        private MasterData CreateMasterData()
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
    }
}
