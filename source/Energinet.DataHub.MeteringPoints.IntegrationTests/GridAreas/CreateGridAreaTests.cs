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

using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.GridAreas;
using Energinet.DataHub.MeteringPoints.Application.GridAreas.Create;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.GridAreas
{
    [IntegrationTest]
    public class CreateGridAreaTests
        : TestHost
    {
        private readonly IGridAreaRepository _gridAreaRepository;

        public CreateGridAreaTests(
            DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _gridAreaRepository = GetService<IGridAreaRepository>();
        }

        [Fact]
        public async Task Created_grid_area_should_be_retrievable_from_repository()
        {
            var request = CreateRequest();

            await SendCommandAsync(request, CancellationToken.None).ConfigureAwait(false);

            var found = await _gridAreaRepository.GetByCodeAsync(request.Code!).ConfigureAwait(false);

            // TODO: compare values
            Assert.NotNull(found);
        }

        private static CreateGridArea CreateRequest()
        {
            return new CreateGridArea(
                SampleData.GridAreaName,
                SampleData.GridAreaCode,
                SampleData.PriceAreaCode,
                SampleData.Transaction);
        }
    }
}
