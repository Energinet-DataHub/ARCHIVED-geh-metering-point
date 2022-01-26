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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class ParentChildCouplingTests : TestHost
    {
        private readonly string _parentGsrnNumber = "570851247381952311";
        private readonly string _childGsrnNumber = SampleData.GsrnNumber;

        public ParentChildCouplingTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Child_is_coupled_to_parent()
        {
            await CreateParentAndChild().ConfigureAwait(false);

            await SendCommandAsync(CreateUpdateRequest()
                with
                {
                    ParentRelatedMeteringPoint = _parentGsrnNumber,
                }).ConfigureAwait(false);

            AssertMasterData(_childGsrnNumber)
                .HasParentMeteringPoint(_parentGsrnNumber);
        }

        [Fact]
        public async Task Child_is_decoupled_from_parent()
        {
            await CoupleChildToParent().ConfigureAwait(false);

            await SendCommandAsync(CreateUpdateRequest()
                with
                {
                    ParentRelatedMeteringPoint = string.Empty,
                }).ConfigureAwait(false);

            AssertMasterData(_childGsrnNumber)
                .HasParentMeteringPoint(null);
        }

        [Fact]
        public async Task Parent_gsrn_number_must_be_valid()
        {
            await CreateParentAndChild().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ParentRelatedMeteringPoint = "invalid gsrn number",
                };
            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Parent_must_exist()
        {
            await CreateChild().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ParentRelatedMeteringPoint = "570851247381952311",
                };
            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        private async Task CreateParentAndChild()
        {
            var createParentCommand = Scenarios.CreateCommand(MeteringPointType.Production) with
            {
                GsrnNumber = _parentGsrnNumber,
            };
            await SendCommandAsync(createParentCommand).ConfigureAwait(false);
            await CreateChild().ConfigureAwait(false);
        }

        private async Task CreateChild()
        {
            var createChildCommand = Scenarios.CreateCommand(MeteringPointType.ElectricalHeating);
            await SendCommandAsync(createChildCommand).ConfigureAwait(false);
        }

        private async Task CoupleChildToParent()
        {
            var createParentCommand = Scenarios.CreateCommand(MeteringPointType.Production) with
            {
                GsrnNumber = _parentGsrnNumber,
            };
            await SendCommandAsync(createParentCommand).ConfigureAwait(false);
            var createChildCommand = Scenarios.CreateCommand(MeteringPointType.ElectricalHeating)
            with
            {
                ParentRelatedMeteringPoint = _parentGsrnNumber,
            };
            await SendCommandAsync(createChildCommand).ConfigureAwait(false);
        }
    }
}
