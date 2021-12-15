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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CreateMeteringPoints
{
    [IntegrationTest]
    public class ParentChildTests : TestHost
    {
        public ParentChildTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Parent_can_act_as_parent()
        {
            var createInvalidParentCommand = Scenarios.CreateCommand(MeteringPointType.NetConsumption) with
            {
                GsrnNumber = "570851247381952311",
            };
            await SendCommandAsync(createInvalidParentCommand).ConfigureAwait(false);

            var createChildCommand = Scenarios.CreateCommand(MeteringPointType.NetConsumption)
                with
                {
                    ParentRelatedMeteringPoint = createInvalidParentCommand.GsrnNumber,
                };
            await SendCommandAsync(createChildCommand).ConfigureAwait(false);

            AssertValidationError("D18");
        }

        [Fact]
        public async Task Parent_must_exist()
        {
            var createChildCommand = Scenarios.CreateCommand(MeteringPointType.NetConsumption)
                with
                {
                    ParentRelatedMeteringPoint = "570851247381952311",
                };
            await SendCommandAsync(createChildCommand).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Parent_and_child_must_be_in_same_grid_area()
        {
            var createInvalidParentCommand = Scenarios.CreateCommand(MeteringPointType.Production) with
            {
                MeteringGridArea = "870",
                GsrnNumber = "570851247381952311",
            };
            await SendCommandAsync(createInvalidParentCommand).ConfigureAwait(false);

            var createChildCommand = Scenarios.CreateCommand(MeteringPointType.NetConsumption)
                with
                {
                    MeteringGridArea = "871",
                    ParentRelatedMeteringPoint = createInvalidParentCommand.GsrnNumber,
                };
            await SendCommandAsync(createChildCommand).ConfigureAwait(false);

            AssertValidationError("D46");
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.InternalUse))]
        [InlineData(nameof(MeteringPointType.NetConsumption))]
        public async Task Only_group_5_metering_points_can_be_coupled_to_group_2_metering_point(string childMeteringPointType)
        {
            var createInvalidParentCommand = Scenarios.CreateCommand(MeteringPointType.Exchange) with
            {
                GsrnNumber = "570851247381952311",
            };
            await SendCommandAsync(createInvalidParentCommand).ConfigureAwait(false);

            var createChildCommand = Scenarios.CreateCommand(EnumerationType.FromName<MeteringPointType>(childMeteringPointType))
                with
                {
                    ParentRelatedMeteringPoint = createInvalidParentCommand.GsrnNumber,
                };
            await SendCommandAsync(createChildCommand).ConfigureAwait(false);

            AssertValidationError("D18");
        }

        [Fact]
        public async Task Cannot_couple_group_5_metering_points_to_group_1_metering_points()
        {
            var createInvalidParentCommand = Scenarios.CreateCommand(MeteringPointType.Production) with
            {
                GsrnNumber = "570851247381952311",
            };
            await SendCommandAsync(createInvalidParentCommand).ConfigureAwait(false);

            var createChildCommand = Scenarios.CreateCommand(MeteringPointType.ExchangeReactiveEnergy)
                with
                {
                    ParentRelatedMeteringPoint = createInvalidParentCommand.GsrnNumber,
                };
            await SendCommandAsync(createChildCommand).ConfigureAwait(false);

            AssertValidationError("D18");
        }
    }
}
