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
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class ProductTypeTests : TestHost
    {
        public ProductTypeTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Product_type_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ProductType = ProductType.Tariff.Name,
                };
            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasProductType(ProductType.Tariff);
        }

        [Fact]
        public async Task Input_value_must_valid()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ProductType = "Invalid value",
                };
            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E29");
        }

        [Fact]
        public async Task Reject_if_configuration_is_not_valid()
        {
            await SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ProductType = ProductType.PowerActive.Name,
                };
            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E29");
        }

        [Fact]
        public async Task Cannot_removed_if_field_is_required()
        {
            await SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ProductType = string.Empty,
                };
            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02");
        }
    }
}