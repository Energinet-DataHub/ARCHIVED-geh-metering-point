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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeMasterData.MasterDataUpdated;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class MeteringConfigurationTests : TestHost
    {
        public MeteringConfigurationTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Metering_method_cannot_be_removed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeteringMethod = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D64");
        }

        [Fact]
        public async Task Metering_configuration_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeteringMethod = MeteringMethod.Physical.Name,
                    MeterNumber = "123",
                };

            await SendCommandAsync(request).ConfigureAwait(false);
            var message = AssertOutboxMessageAndReturnMessage<MasterDataWasUpdatedIntegrationEvent>();

            Assert.Equal(message?.MeteringMethod, MeteringMethod.Physical.Name);
            AssertMasterData()
                .HasMeteringConfiguration(MeteringMethod.Physical, "123");
        }

        [Fact]
        public async Task Meter_is_required_when_changing_method_to_physical()
        {
            await SendCommandAsync(Scenarios.CreateVirtualConsumptionMeteringPoint()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeteringMethod = MeteringMethod.Physical.Name, MeterNumber = null,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D31");
        }

        [Fact]
        public async Task Metering_method_value_must_be_valid()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeteringMethod = "invalid value",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02");
        }

        [Fact]
        public async Task Meter_number_must_be_valid()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()
                with
                {
                    MeteringMethod = MeteringMethod.Physical.Name,
                    MeterNumber = "123",
                }).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeterNumber = "123456789012345678901234567890",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Reject_if_metering_configuration_is_invalid()
        {
            await SendCommandAsync(Scenarios.CreateTotalComsumptionMeteringPoint()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeteringMethod = MeteringMethod.Physical.Name,
                    MeterNumber = "123456789",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D37");
        }
    }
}
