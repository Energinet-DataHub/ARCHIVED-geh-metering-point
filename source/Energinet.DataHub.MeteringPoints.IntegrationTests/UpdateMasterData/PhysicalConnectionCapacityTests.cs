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

using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class PhysicalConnectionCapacityTests : TestHost
    {
        public PhysicalConnectionCapacityTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Capacity_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    PhysicalConnectionCapacity = "1",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasCapacity(1);
        }

        [Theory]
        [InlineData("1234567891011")]
        [InlineData("12345678A")]
        public async Task Capacity_must_be_rejected_if_format_is_incorrect(string capacity)
        {
            await SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    PhysicalConnectionCapacity = capacity,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Capacity_is_mandatory_if_net_settlement_group_is_not_zero()
        {
            await SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    PhysicalConnectionCapacity = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D56");
        }
    }
}
