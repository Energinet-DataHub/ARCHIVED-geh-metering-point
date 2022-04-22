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

using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeMasterData.MasterDataUpdated;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class NetSettlementGroupTests : TestHost
    {
        public NetSettlementGroupTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Net_settlement_group_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.Zero.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);
            var message = AssertOutboxMessageAndReturnMessage<MasterDataWasUpdatedIntegrationEvent>();

            Assert.Equal(message?.NetSettlementGroup, NetSettlementGroup.Zero.Name);
            AssertMasterData()
                .HasNetSettlementGroup(NetSettlementGroup.Zero);
        }

        [Fact]
        public async Task Reject_if_value_is_invalid()
        {
            await SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    NetSettlementGroup = "Invalid value",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D62");
        }

        [Fact]
        public async Task Cannot_be_removed_if_required()
        {
            await SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    NetSettlementGroup = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D62");
        }
    }
}
