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
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class PowerLimitTests : TestHost
    {
        public PowerLimitTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Power_limit_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction() with
            {
                MaximumCurrent = "1",
                MaximumPower = "1",
            }).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MaximumCurrent = "2",
                    MaximumPower = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasPowerLimit(null, 2);
        }

        [Fact]
        public async Task Power_limit_is_changed_if_null()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction() with
            {
                MaximumCurrent = null,
                MaximumPower = null,
            }).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MaximumCurrent = "2",
                    MaximumPower = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasPowerLimit(null, 2);
        }

        [Fact]
        public async Task Reject_if_max_current_input_is_invalid()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction() with
            {
                MaximumCurrent = "1",
                MaximumPower = "1",
            }).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MaximumCurrent = "1234567",
                    MaximumPower = "123",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Reject_if_max_power_input_is_invalid()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction() with
            {
                MaximumCurrent = "1",
                MaximumPower = "1",
            }).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MaximumCurrent = "123",
                    MaximumPower = "1234567",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }
    }
}
