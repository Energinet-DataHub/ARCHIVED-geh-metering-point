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
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.MarketMeteringPoints
{
    [UnitTest]
    public class MarketMeteringPointTests : TestBase
    {
        private readonly SystemDateTimeProviderStub _systemDateTimeProvider;

        public MarketMeteringPointTests()
        {
            _systemDateTimeProvider = new SystemDateTimeProviderStub();
        }

        [Fact]
        public void Should_set_energy_supplier_details()
        {
            var marketMeteringPoint = CreateMarketMeteringPoint();
            marketMeteringPoint.SetEnergySupplierDetails(EnergySupplierDetails.Create(_systemDateTimeProvider.Now()));

            Assert.Contains(marketMeteringPoint.DomainEvents, e => e is EnergySupplierDetailsChanged);
        }

        [Fact]
        public void Does_not_change_energy_supplier_details_when_details_are_the_same()
        {
            var marketMeteringPoint = CreateMarketMeteringPoint();
            marketMeteringPoint.SetEnergySupplierDetails(EnergySupplierDetails.Create(_systemDateTimeProvider.Now()));

            Assert.Equal(1, marketMeteringPoint.DomainEvents.Count(e => e is EnergySupplierDetailsChanged));
        }

        [Fact]
        public void Should_return_error_when_geo_info_reference_is_undefined()
        {
            var address = Address.Create(
                SampleData.StreetName,
                SampleData.StreetCode,
                SampleData.BuildingNumber,
                SampleData.CityName,
                SampleData.CitySubdivision,
                SampleData.PostCode,
                EnumerationType.FromName<CountryCode>(SampleData.CountryCode),
                SampleData.Floor,
                SampleData.Room,
                SampleData.MunicipalityCode,
                SampleData.IsActualAddress,
                null);

            var details = CreateConsumptionDetails()
                with
                {
                    Address = address,
                };

            var checkResult = MarketMeteringPoint.CanCreate(details);

            AssertError<GeoInfoReferenceIsRequiredRuleError>(checkResult, true);
        }

        private static MarketMeteringPoint CreateMarketMeteringPoint()
        {
            var address = Address.Create(
                SampleData.StreetName,
                SampleData.StreetCode,
                SampleData.BuildingNumber,
                SampleData.CityName,
                SampleData.CitySubdivision,
                SampleData.PostCode,
                EnumerationType.FromName<CountryCode>(SampleData.CountryCode),
                SampleData.Floor,
                SampleData.Room,
                SampleData.MunicipalityCode,
                SampleData.IsActualAddress,
                SampleData.GeoInfoReference);

            var builder = MasterDataBuilder(MeteringPointType.Consumption)
                .WithNetSettlementGroup(NetSettlementGroup.Six.Name)
                .WithMeteringConfiguration(SampleData.SubTypeName, SampleData.MeterNumber)
                .WithScheduledMeterReadingDate(null!)
                .WithReadingPeriodicity(ReadingOccurrence.Hourly.Name)
                .WithPowerLimit(0, 0)
                .EffectiveOn(SampleData.EffectiveDate)
                .WithCapacity(1.2)
                .WithConnectionType(ConnectionType.Installation.Name)
                .WithDisconnectionType(DisconnectionType.Remote.Name)
                .WithAssetType(AssetType.HydroelectricPower.Name);

            return new MarketMeteringPointMock(
                MeteringPointId.New(),
                GsrnNumber.Create(SampleData.GsrnNumber),
                address,
                EnumerationType.FromName<MeteringPointType>(SampleData.TypeName),
                new GridAreaLinkId(Guid.Parse(SampleData.GridAreaLinkId)),
                EffectiveDate.Create(SampleData.EffectiveDate),
                ConnectionType.Installation,
                DisconnectionType.Remote,
                NetSettlementGroup.Six,
                MeteringConfiguration.Create(EnumerationType.FromName<MeteringMethod>(SampleData.SubTypeName), MeterId.Create(SampleData.MeterNumber)),
                builder.Build());
        }
    }
}
