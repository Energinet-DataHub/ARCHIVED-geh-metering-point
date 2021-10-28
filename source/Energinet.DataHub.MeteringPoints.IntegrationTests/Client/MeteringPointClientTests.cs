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
using Energinet.DataHub.MeteringPoints.Client;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.Client
{
    [IntegrationTest]
    public class MeteringPointClientTests : WebApiHost
    {
        private readonly WebApiFactory _factory;

        public MeteringPointClientTests(WebApiFactory factory, DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_metering_point_by_id_should_not_be_null()
        {
            // Arrange
            var httpClient = _factory.CreateClient();
            var meteringPointClient = MeteringPointClientFactory.CreateClient(httpClient);

            // Act
            var response = await meteringPointClient.GetMeteringPointByGsrnAsync("571313180400013469").ConfigureAwait(false);

            // Assert
            response.Should().NotBeNull();
        }
    }
}
