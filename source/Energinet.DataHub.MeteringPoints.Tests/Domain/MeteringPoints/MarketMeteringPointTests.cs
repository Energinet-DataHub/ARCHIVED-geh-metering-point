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

using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints
{
    #pragma warning disable
    [UnitTest]
    public class MarketMeteringPointTests
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

        private static MarketMeteringPoint CreateMarketMeteringPoint()
        {
            return new MarketMeteringPointMock(
                MeteringPointId.New(),
                GsrnNumber.Create(SampleData.GsrnNumber),
                Address.Create(SampleData.StreetName, "0001", "1000", SampleData.PostCode, SampleData.CityName, SampleData.CountryCode),
                EnumerationType.FromName<MeteringPointSubType>(SampleData.SubTypeName),
                EnumerationType.FromName<MeteringPointType>(SampleData.TypeName),
                GridAreaId.New(),
                GsrnNumber.Create(SampleData.PowerPlant),
                SampleData.LocationDescription,
                MeasurementUnitType.KWh,
                SampleData.MeterNumber,
                ReadingOccurrence.Hourly,
                SampleData.MaximumCurrent,
                SampleData.MaximumPower,
                SampleData.OccurenceDate,
                parentRelatedMeteringPoint: null);
        }
    }
}
