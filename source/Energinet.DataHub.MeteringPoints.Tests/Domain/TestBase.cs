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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain
{
    public abstract class TestBase
    {
        protected static void AssertError<TRuleError>(BusinessRulesValidationResult rulesValidationResult, bool errorExpected)
        {
            if (rulesValidationResult == null) throw new ArgumentNullException(nameof(rulesValidationResult));
            var hasError = rulesValidationResult.Errors.Any(error => error is TRuleError);
            Assert.Equal(errorExpected, hasError);
        }

        protected static MeteringPoint CreateMeteringPoint(MeteringPointType type, IMasterDataBuilder? masterDataBuilder = null)
        {
            var builder = masterDataBuilder ?? MasterDataBuilder(type);
            return MeteringPoint.Create(
                MeteringPointId.New(),
                GsrnNumber.Create(SampleData.GsrnNumber),
                type,
                new GridAreaLinkId(Guid.Parse(SampleData.GridAreaLinkId)),
                EffectiveDate.Create(SampleData.EffectiveDate),
                builder.Build());
        }

        protected static IMasterDataBuilder MasterDataBuilder(MeteringPointType meteringPointType)
        {
            var builder = new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(meteringPointType))
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

            return builder;
        }

        protected static IMasterDataBuilder MasterDataBuilderForSpecial()
        {
            return MasterDataBuilder(MeteringPointType.VEProduction)
                .WithMeasurementUnitType(MeasurementUnitType.KWh.Name)
                .WithProductType(ProductType.EnergyActive.Name);
        }

        protected static IMasterDataBuilder MasterDataBuilderForProduction()
        {
            var builder = new MasterDataBuilder(new MasterDataFieldSelector().GetMasterDataFieldsFor(MeteringPointType.Production))
                .WithMeasurementUnitType(MeasurementUnitType.KWh.Name)
                .WithProductType(ProductType.EnergyActive.Name)
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
                .WithMeteringConfiguration(MeteringMethod.Physical.Name, "1");

            return builder;
        }

        protected static IMasterDataBuilder MasterDataBuilderForExchange()
        {
            return MasterDataBuilder(MeteringPointType.Exchange)
                .WithMeasurementUnitType(MeasurementUnitType.KWh.Name)
                .WithProductType(ProductType.EnergyActive.Name);
        }

        protected static IMasterDataBuilder MasterDataBuilderForConsumption()
        {
            return MasterDataBuilder(MeteringPointType.Consumption)
                .WithMeasurementUnitType(MeasurementUnitType.KWh.Name)
                .WithProductType(ProductType.EnergyActive.Name);
        }

        protected static Address CreateAddress()
        {
            return Address.Create(
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
                geoInfoReference: Guid.NewGuid());
        }

        protected static void AssertContainsValidationError<TValidationError>(BusinessRulesValidationResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            Assert.Contains(result.Errors, error => error is TValidationError);
        }

        protected static void AssertDoesNotContainValidationError<TValidationError>(BusinessRulesValidationResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            Assert.DoesNotContain(result.Errors, error => error is TValidationError);
        }
    }
}
