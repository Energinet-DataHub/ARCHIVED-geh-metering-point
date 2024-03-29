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
    public class ChangeTests : TestHost
    {
        public ChangeTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Metering_point_updated_shows_in_process_overview()
        {
            await CreateMeteringPointAsync().ConfigureAwait(false);
            var request = CreateUpdateRequest();

            await SendCommandAsync(request).ConfigureAwait(false);

            await AssertProcessOverviewAsync(
                    SampleData.GsrnNumber,
                    "BRS-006",
                    "RequestUpdateMeteringPoint",
                    "ConfirmUpdateMeteringPoint")
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task It_is_not_allowed_to_provide_to_grid_area()
        {
            await SendCommandAsync(CreateUpdateRequest()
                with
                {
                    ToGrid = "871",
                }).ConfigureAwait(false);

            AssertValidationError("D46");
        }

        [Fact]
        public async Task It_is_not_allowed_to_provide_from_grid_area()
        {
            await SendCommandAsync(CreateUpdateRequest()
                with
                {
                    FromGrid = "871",
                }).ConfigureAwait(false);

            AssertValidationError("D46");
        }

        [Fact]
        public async Task Transaction_id_is_required()
        {
            await SendCommandAsync(TestUtils.CreateRequest()
                with
                {
                    TransactionId = string.Empty,
                }).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Metering_point_must_exist()
        {
            await SendCommandAsync(TestUtils.CreateRequest()).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Theory]
        [InlineData("invalid_gsrn_number")]
        [InlineData("")]
        public async Task Gsrn_number_is_required(string gsrnNumber)
        {
            await SendCommandAsync(TestUtils.CreateRequest()
                with
                {
                    GsrnNumber = gsrnNumber,
                }).ConfigureAwait(false);

            AssertValidationError("D57");
        }

        [Fact]
        public async Task Grid_operator_is_the_owner_of_the_metering_point()
        {
            await CreateMeteringPointAsync().ConfigureAwait(false);

            SetGridOperatorAsAuthenticatedUser("820000000140x"); // This is not the owner of this metering point
            await SendCommandAsync(TestUtils.CreateRequest()).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Can_not_change_when_metering_point_is_closed_down()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);
            await CloseDownMeteringPointAsync().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeterNumber = "1",
                };
            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D16");
        }

        private Task CreateMeteringPointAsync()
        {
            return SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand());
        }
    }
}
