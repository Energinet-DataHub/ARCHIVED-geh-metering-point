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
        public void Product_type_is_changed()
        {
            var meteringPoint = CreateMeteringPoint(MeteringPointType.Consumption);
            var builder = new MasterDataUpdater(new MasterDataFieldSelector().GetMasterDataFieldsFor(MeteringPointType.Consumption), meteringPoint.MasterData);

            builder.WithProductType(ProductType.PowerReactive.Name);
            var updatedMasterData = builder.Build();

            Assert.Equal(ProductType.PowerReactive, updatedMasterData.ProductType);
        }
    }

    public class MasterDataUpdater : MasterDataBuilderBase, IMasterDataBuilder
    {
        public MasterDataUpdater(IEnumerable<MasterDataField> masterDataFields, MasterData currentMasterData)
            : base(masterDataFields)
        {
            if (currentMasterData == null) throw new ArgumentNullException(nameof(currentMasterData));
            SetValue(nameof(MasterData.ProductType), currentMasterData.ProductType);
        }

        public BusinessRulesValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public IMasterDataBuilder WithNetSettlementGroup(string netSettlementGroup)
        {
            throw new NotImplementedException();
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
            if (string.IsNullOrEmpty(productType) == false)
            {
                SetValueIfValid(
                    nameof(MasterData.ProductType),
                    BusinessRulesValidationResult.Valid,
                    () => EnumerationType.FromName<ProductType>(productType));
            }
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
            var valueItem = GetMasterValueItem<T>(valueName);
            valueItem.SetValue(validator, creator);
        }
    }
}
