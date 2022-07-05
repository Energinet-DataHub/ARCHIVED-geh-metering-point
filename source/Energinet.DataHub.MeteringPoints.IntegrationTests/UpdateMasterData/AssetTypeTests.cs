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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class AssetTypeTests : TestHost
    {
        public AssetTypeTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Asset_type_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    AssetType = AssetType.Boiler.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasAssetType(AssetType.Boiler);
        }

        [Fact]
        public async Task Reject_if_asset_type_value_is_invalid()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    AssetType = "invalid value",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D59");
        }

        [Fact]
        public async Task Asset_type_cannot_be_removed_if_required()
        {
            await SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    AssetType = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D59");
        }

        [Theory]
        [InlineData(nameof(AssetType.NoTechnology), nameof(AssetType.GasTurbine))]
        [InlineData(nameof(AssetType.GasTurbine), null)]
        [InlineData(nameof(AssetType.Boiler), nameof(AssetType.GasTurbine))]
        [InlineData(nameof(AssetType.Boiler), "")]
        public async Task Asset_type_can_be_updated(string oldAssetType, string newAssetType)
        {
            var meteringPoint = Scenarios.CreateConsumptionMeteringPointCommand()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.Zero.Name,
                    AssetType = oldAssetType,
                    ConnectionType = null,
                    ScheduledMeterReadingDate = null,
                };

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    AssetType = newAssetType,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D59", false);
        }

        [Fact]
        public async Task? VE_production_metering_point_can_change_asset_type_to_no_technology()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = await UpdateMeteringpoint().ConfigureAwait(false);

            AssertAssetType(request);
        }

        [Fact]
        public async Task? Surplus_production_metering_point_can_change_asset_type_to_no_technology()
        {
            var meteringPoint = Scenarios.CreateSurplusProduction();

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            var request = await UpdateMeteringpoint().ConfigureAwait(false);

            AssertAssetType(request);
        }

        [Fact]
        public async Task? Net_production_metering_point_can_change_asset_type_to_no_technology()
        {
            var meteringPoint = Scenarios.CreateCommand(MeteringPointType.NetProduction);

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            var request = await UpdateMeteringpoint().ConfigureAwait(false);

            AssertAssetType(request);
        }

        [Fact]
        public async Task? Supply_to_grid_metering_point_can_change_asset_type_to_no_technology()
        {
            var meteringPoint = Scenarios.CreateCommand(MeteringPointType.SupplyToGrid);

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            var request = await UpdateMeteringpoint().ConfigureAwait(false);

            AssertAssetType(request);
        }

        [Fact]
        public async Task? Consumption_from_grid_metering_point_can_change_asset_type_to_no_technology()
        {
            var meteringPoint = Scenarios.CreateCommand(MeteringPointType.ConsumptionFromGrid);

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            var request = await UpdateMeteringpoint().ConfigureAwait(false);

            AssertAssetType(request);
        }

        [Fact]
        public async Task? Wholesale_services_metering_point_can_change_asset_type_to_no_technology()
        {
            var meteringPoint = Scenarios.CreateCommand(MeteringPointType.WholesaleServices);

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            var request = await UpdateMeteringpoint().ConfigureAwait(false);

            AssertAssetType(request);
        }

        [Fact]
        public async Task? Own_production_metering_point_can_change_asset_type_to_no_technology()
        {
            var meteringPoint = Scenarios.CreateCommand(MeteringPointType.OwnProduction);

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            var request = await UpdateMeteringpoint().ConfigureAwait(false);

            AssertAssetType(request);
        }

        [Fact]
        public async Task? Net_from_grid_metering_point_can_change_asset_type_to_no_technology()
        {
            var meteringPoint = Scenarios.CreateCommand(MeteringPointType.NetFromGrid);

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            var request = await UpdateMeteringpoint().ConfigureAwait(false);

            AssertAssetType(request);
        }

        [Fact]
        public async Task? Net_to_grid_metering_point_can_change_asset_type_to_no_technology()
        {
            var meteringPoint = Scenarios.CreateCommand(MeteringPointType.NetToGrid);

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            var request = await UpdateMeteringpoint().ConfigureAwait(false);

            AssertAssetType(request);
        }

        [Fact]
        public async Task? Total_consumption_metering_point_can_change_asset_type_to_no_technology()
        {
            var meteringPoint = Scenarios.CreateCommand(MeteringPointType.TotalConsumption);

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            var request = await UpdateMeteringpoint().ConfigureAwait(false);

            AssertAssetType(request);
        }

        [Fact]
        public async Task? Other_consumption_metering_point_can_change_asset_type_to_no_technology()
        {
            var meteringPoint = Scenarios.CreateCommand(MeteringPointType.OtherConsumption)
                with
                {
                    MeteringMethod = MeteringMethod.Physical.Name,
                    MeterNumber = "1234",
                };

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            var request = await UpdateMeteringpoint().ConfigureAwait(false);

            AssertAssetType(request);
        }

        [Fact]
        public async Task? Other_production_metering_point_can_change_asset_type_to_no_technology()
        {
            var meteringPoint = Scenarios.CreateCommand(MeteringPointType.OtherProduction)
                with
                {
                    MeteringMethod = MeteringMethod.Physical.Name,
                    MeterNumber = "1234",
                };

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            var request = await UpdateMeteringpoint().ConfigureAwait(false);

            AssertAssetType(request);
        }

        [Fact]
        public async Task Consumption_metering_point_with_net_settlement_group_0_can_change_asset_type_to_no_technology()
        {
            var meteringPoint = Scenarios.CreateConsumptionMeteringPointCommand()
                with
                {
                    NetSettlementGroup = nameof(NetSettlementGroup.Zero),
                    ConnectionType = string.Empty,
                    ScheduledMeterReadingDate = string.Empty,
                };

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            var request = await UpdateMeteringpoint().ConfigureAwait(false);

            AssertAssetType(request);
        }

        [Fact]
        public async Task Consumption_metering_point_with_net_settlement_group_other_than_0_cannot_change_asset_type_to_no_technology()
        {
            var meteringPoint = Scenarios.CreateConsumptionMeteringPointCommand()
                with
                {
                    NetSettlementGroup = nameof(NetSettlementGroup.Six),
                };

            await SendCommandAsync(meteringPoint).ConfigureAwait(false);

            await UpdateMeteringpoint().ConfigureAwait(false);

            AssertValidationError("D60", true);
        }

        private async Task<MasterDataDocument> UpdateMeteringpoint()
        {
            var request = CreateUpdateRequest()
                with
                {
                    AssetType = nameof(AssetType.NoTechnology),
                };

            await SendCommandAsync(request).ConfigureAwait(false);
            return request;
        }

        private void AssertAssetType(MasterDataDocument request)
        {
            var masterData = AssertMasterData(request.GsrnNumber);
            masterData.HasAssetType(AssetType.NoTechnology);
        }
    }
}
