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
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Microsoft.EntityFrameworkCore;
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
        public async Task Reject_when_parent_can_not_act_as_parent()
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
        public async Task Parent_gsrn_number_is_must_be_valid()
        {
            var createChildCommand = Scenarios.CreateCommand(MeteringPointType.NetConsumption)
                with
                {
                    ParentRelatedMeteringPoint = "invalid gsrn number",
                };
            await SendCommandAsync(createChildCommand).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Cannot_act_as_parent()
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
            var createParentCommand = Scenarios.CreateCommand(MeteringPointType.Production) with
            {
                MeteringGridArea = "870",
                GsrnNumber = "570851247381952311",
            };
            await SendCommandAsync(createParentCommand).ConfigureAwait(false);

            var createChildCommand = Scenarios.CreateCommand(MeteringPointType.NetConsumption)
                with
                {
                    MeteringGridArea = "871",
                    ParentRelatedMeteringPoint = createParentCommand.GsrnNumber,
                };
            await SendCommandAsync(createChildCommand).ConfigureAwait(false);

            AssertValidationError("D46");
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.InternalUse))]
        [InlineData(nameof(MeteringPointType.NetConsumption))]
        public async Task Cannot_couple_group_3_or_4_metering_points_to_group_2_metering_points(string childMeteringPointType)
        {
            var createParentCommand = Scenarios.CreateCommand(MeteringPointType.Exchange) with
            {
                GsrnNumber = "570851247381952311",
            };
            await SendCommandAsync(createParentCommand).ConfigureAwait(false);

            var createChildCommand = Scenarios.CreateCommand(EnumerationType.FromName<MeteringPointType>(childMeteringPointType))
                with
                {
                    ParentRelatedMeteringPoint = createParentCommand.GsrnNumber,
                };
            await SendCommandAsync(createChildCommand).ConfigureAwait(false);

            AssertValidationError("D18");
        }

        [Fact]
        public async Task Cannot_couple_group_5_metering_points_to_group_1_metering_points()
        {
            var createParentCommand = Scenarios.CreateCommand(MeteringPointType.Production) with
            {
                GsrnNumber = "570851247381952311",
            };
            await SendCommandAsync(createParentCommand).ConfigureAwait(false);

            var createChildCommand = Scenarios.CreateExchangeReactiveEnergy()
                with
                {
                    ParentRelatedMeteringPoint = createParentCommand.GsrnNumber,
                };
            await SendCommandAsync(createChildCommand).ConfigureAwait(false);

            AssertValidationError("D18");
        }

        [Fact]
        public async Task Child_is_coupled_to_parent()
        {
            var createParentCommand = Scenarios.CreateCommand(MeteringPointType.Production) with
            {
                GsrnNumber = "570851247381952311",
            };
            await SendCommandAsync(createParentCommand).ConfigureAwait(false);

            var createChildCommand = Scenarios.CreateCommand(MeteringPointType.ElectricalHeating)
                with
                {
                    ParentRelatedMeteringPoint = createParentCommand.GsrnNumber,
                };
            await SendCommandAsync(createChildCommand).ConfigureAwait(false);

            var context = GetService<MeteringPointContext>();
            var found = await context.MeteringPoints.FromSqlInterpolated($"SELECT * FROM [dbo].[MeteringPoints] WHERE ParentRelatedMeteringPoint IS NOT NULL")
                .SingleOrDefaultAsync(x => x.GsrnNumber.Equals(GsrnNumber.Create(createChildCommand.GsrnNumber)))
                .ConfigureAwait(false);
            Assert.NotNull(found);
        }
    }
}
