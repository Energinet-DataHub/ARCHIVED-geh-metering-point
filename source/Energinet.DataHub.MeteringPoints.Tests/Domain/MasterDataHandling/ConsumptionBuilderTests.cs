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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MasterDataHandling
{
    [UnitTest]
    public class ConsumptionBuilderTests
    {
        [Theory]
        [InlineData(nameof(MeteringPointType.ElectricalHeating), nameof(ProductType.EnergyActive))]
        [InlineData(nameof(MeteringPointType.NetConsumption), nameof(ProductType.EnergyActive))]
        [InlineData(nameof(MeteringPointType.NetProduction), nameof(ProductType.EnergyActive))]
        [InlineData(nameof(MeteringPointType.OtherConsumption), nameof(ProductType.EnergyActive))]
        [InlineData(nameof(MeteringPointType.OtherProduction), nameof(ProductType.EnergyActive))]
        [InlineData(nameof(MeteringPointType.OwnProduction), nameof(ProductType.EnergyActive))]
        [InlineData(nameof(MeteringPointType.TotalConsumption), nameof(ProductType.EnergyActive))]
        [InlineData(nameof(MeteringPointType.ConsumptionFromGrid), nameof(ProductType.EnergyActive))]
        [InlineData(nameof(MeteringPointType.WholesaleServices), nameof(ProductType.EnergyActive))]
        [InlineData(nameof(MeteringPointType.GridLossCorrection), nameof(ProductType.EnergyActive))]
        [InlineData(nameof(MeteringPointType.ExchangeReactiveEnergy), nameof(ProductType.EnergyReactive))]
        public void Product_type_has_correct_default_value(string type, string expectedDefaultValue)
        {
            var fields = new MasterDataFieldSelector();
            var sut = new MasterDataBuilder(fields.GetMasterDataFieldsFor(EnumerationType.FromName<MeteringPointType>(type)))
                .WithProductType(ProductType.PowerReactive.Name)
                .Build();

            Assert.Equal(EnumerationType.FromName<ProductType>(expectedDefaultValue), sut.ProductType);
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
