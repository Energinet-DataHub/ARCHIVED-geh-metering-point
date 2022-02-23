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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.EntryPoints.WebApi.MeteringPoints.Queries;
using FluentAssertions;
using MediatR;
using SimpleInjector;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling
{
    public class AssertHelper
    {
        private readonly Container _container;

        public AssertHelper(
            Container container)
        {
            _container = container;
        }

        internal async Task ProcessOverviewAsync(
            string gsrn,
            string expectedProcessName,
            params string[] expectedProcessSteps)
        {
            var processes = await _container.GetInstance<IRequestHandler<MeteringPointProcessesByGsrnQuery, List<Process>>>()
                .Handle(new MeteringPointProcessesByGsrnQuery(gsrn), CancellationToken.None)
                .ConfigureAwait(false);

            processes.Should().ContainSingle(process => process.Name == expectedProcessName, $"a single process with name {expectedProcessName} was expected")
                .Which.Details.Select(detail => detail.Name).Should().ContainInOrder(expectedProcessSteps);
        }
    }
}
