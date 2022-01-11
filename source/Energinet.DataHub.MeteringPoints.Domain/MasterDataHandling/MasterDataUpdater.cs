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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling
{
    public class MasterDataUpdater : MasterDataBuilderBase, IMasterDataBuilder
    {
        private readonly List<ValidationError> _validationErrors = new();

        public MasterDataUpdater(IEnumerable<MasterDataField> fields, MasterData currentMasterData)
            : base(fields)
        {
            if (currentMasterData == null) throw new ArgumentNullException(nameof(currentMasterData));
            SetValue(nameof(MasterData.ProductType), currentMasterData.ProductType);
            SetValue(nameof(MasterData.NetSettlementGroup), currentMasterData.NetSettlementGroup);
            SetValue(nameof(MasterData.ConnectionType), currentMasterData.ConnectionType);
            SetValue(nameof(MasterData.MeteringConfiguration), currentMasterData.MeteringConfiguration);
            SetValue(nameof(MasterData.Address), currentMasterData.Address);
            SetValue(nameof(MasterData.UnitType), currentMasterData.UnitType);
        }

        public BusinessRulesValidationResult Validate()
        {
            _validationErrors.Clear();
            _validationErrors.AddRange(AllValueValidationErrors());

            AddValidationErrorIfRequiredFieldIsMissing<MeasurementUnitType>(nameof(MasterData.UnitType), new UnitTypeIsRequired());

            return new BusinessRulesValidationResult(_validationErrors);
        }

        public MasterData Build()
        {
            RemoveValueIfNotApplicable<ScheduledMeterReadingDate>(
                nameof(MasterData.ScheduledMeterReadingDate),
                () => GetValue<NetSettlementGroup>(nameof(MasterData.NetSettlementGroup)) != NetSettlementGroup.Six);
            RemoveValueIfNotApplicable<ConnectionType>(
                nameof(MasterData.ConnectionType),
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
                    validationResult = meteringMethod == MeteringMethod.Physical ? MeteringConfiguration.CheckRules(meteringMethod, string.IsNullOrEmpty(meterNumber) ? MeterId.Empty() : MeterId.Create(meterNumber)) : MeteringConfiguration.CheckRules(meteringMethod, MeterId.Empty());

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

        public IMasterDataBuilder WithAddress(string? streetName = null, string? streetCode = null, string? buildingNumber = null, string? city = null, string? citySubDivision = null, string? postCode = null, CountryCode? countryCode = null, string? floor = null, string? room = null, int? municipalityCode = null, bool? isActual = null, Guid? geoInfoReference = null, string? locationDescription = null)
        {
            SetValueIfValid(
                nameof(MasterData.Address),
                () => Address.CheckRules(streetName, streetCode, buildingNumber, city, citySubDivision, postCode, countryCode, floor, room, municipalityCode, locationDescription),
                () =>
                {
                    var currentAddress = GetValue<Address>(nameof(MasterData.Address));
                    var address = Address.Create(streetName, streetCode, buildingNumber, city, citySubDivision, postCode, countryCode, floor, room, municipalityCode, isActual, geoInfoReference, locationDescription);
                    return currentAddress.MergeFrom(address);
                });

            return this;
        }

        public IMasterDataBuilder WithMeasurementUnitType(string? measurementUnitType)
        {
            if (measurementUnitType is null) return this;
            if (measurementUnitType.Length == 0) SetValue<MeasurementUnitType>(nameof(MasterData.UnitType), null);
            if (measurementUnitType.Length > 0)
            {
                SetValueIfValid(
                nameof(MasterData.UnitType),
                () =>
                {
                    if (EnumerationType.GetAll<MeasurementUnitType>()
                        .Select(item => item.Name)
                        .Contains(measurementUnitType) == false)
                    {
                        return BusinessRulesValidationResult.Failure(new InvalidUnitTypeValue(measurementUnitType));
                    }

                    return BusinessRulesValidationResult.Valid();
                },
                () => EnumerationType.FromName<MeasurementUnitType>(measurementUnitType));
            }

            return this;
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

            SetValueIfValid(
                nameof(MasterData.ProductType),
                BusinessRulesValidationResult.Valid,
                () => EnumerationType.FromName<ProductType>(productType));

            return this;
        }

        public IMasterDataBuilder WithConnectionType(string? connectionType)
        {
            if (connectionType is null) return this;
            if (connectionType.Length == 0) SetValue<ConnectionType>(nameof(MasterData.ConnectionType), null);
            if (connectionType.Length != 0)
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

        public IMasterDataBuilder WithProductionObligation(bool? productionObligation)
        {
            throw new NotImplementedException();
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
    }
}
