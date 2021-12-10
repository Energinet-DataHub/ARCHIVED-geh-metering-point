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

using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
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

        [Theory]
        [InlineData(nameof(MeteringPointType.Consumption), nameof(MeasurementUnitType.KWh))]
        [InlineData(nameof(MeteringPointType.Production), nameof(MeasurementUnitType.KWh))]
        [InlineData(nameof(MeteringPointType.Exchange), nameof(MeasurementUnitType.KWh))]
        [InlineData(nameof(MeteringPointType.VEProduction), nameof(MeasurementUnitType.KVArh))]
        public void Measurement_unit_type_is_correct(string type, string unitType)
        {
            var fields = new MasterDataFieldSelector();
            var sut = new MasterDataBuilder(fields.GetMasterDataFieldsFor(EnumerationType.FromName<MeteringPointType>(type)))
                .WithMeasurementUnitType(unitType)
                .Build();

            Assert.Equal(EnumerationType.FromName<MeasurementUnitType>(unitType), sut.UnitType);
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
}
