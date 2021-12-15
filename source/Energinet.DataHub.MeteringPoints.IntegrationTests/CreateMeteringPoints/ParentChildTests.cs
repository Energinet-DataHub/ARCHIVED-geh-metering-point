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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
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
        public async Task Parent_is_in_group_1_or_2()
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
    }
}
