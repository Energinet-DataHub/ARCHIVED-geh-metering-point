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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeMasterData.MasterDataUpdated;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class UnitTypeTests : TestHost
    {
        public UnitTypeTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Unit_type_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeasureUnitType = MeasurementUnitType.Ampere.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);
            var message = AssertOutboxMessageAndReturnMessage<MasterDataWasUpdatedIntegrationEvent>();

            AssertMasterData()
                .HasUnitType(MeasurementUnitType.Ampere);
            Assert.Equal(message?.UnitType, MeasurementUnitType.Ampere.Name);
        }

        [Fact]
        public async Task Unit_type_input_value_must_be_valid()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeasureUnitType = "invalid value",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E73");
        }

        [Fact]
        public async Task Cannot_be_removed_if_required_field()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeasureUnitType = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E73");
        }

        [Fact]
        public async Task Must_be_valid_according_to_metering_point_type()
        {
            await SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeasureUnitType = MeasurementUnitType.Ampere.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E73");
        }
    }
}
