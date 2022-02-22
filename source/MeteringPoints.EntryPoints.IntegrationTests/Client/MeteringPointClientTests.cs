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

using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Client;
using Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Tooling.WebApi;
using Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Tooling.WebApi.Hosts;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Client
{
    [IntegrationTest]
    public class MeteringPointClientTests : WebApiHostWithoutSecurity
    {
        private readonly WebApiWithoutSecurityFactory _webApiFactory;

        public MeteringPointClientTests(WebApiWithoutSecurityFactory webApiFactory, DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _webApiFactory = webApiFactory;
        }

        [Fact]
        public async Task Get_metering_point_processes_by_gsrn_should_not_be_null_or_empty()
        {
            // Arrange
            var sut = await CreateMeteringPointClient().ConfigureAwait(false);

            // Act
            var response = await sut.GetProcessesByGsrnAsync("571313157178361184").ConfigureAwait(false);

            // Assert
            response.Should().NotBeNull();
            response.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Get_metering_point_processes_by_gsrn_should_order_steps_correct()
        {
            // Arrange
            var sut = await CreateMeteringPointClient().ConfigureAwait(false);

            // Act
            var response = await sut.GetProcessesByGsrnAsync("571313157178361184").ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                response.Should().HaveCount(2);
                response.First().Details[0].Name.Should().StartWith("Request");
                response.First().Details[1].Name.Should().StartWith("Reject");
                response.Last().Details[0].Name.Should().StartWith("Request");
                response.Last().Details[1].Name.Should().StartWith("Confirm");
            }
        }

        [Fact]
        public async Task Get_metering_point_processes_by_gsrn_should_contain_errors_in_reject_steps()
        {
            // Arrange
            var sut = await CreateMeteringPointClient().ConfigureAwait(false);

            // Act
            var response = await sut.GetProcessesByGsrnAsync("571313157178361184").ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                var rejectedDetails = response.First().Details[1];
                rejectedDetails.Name.Should().StartWith("Reject");
                rejectedDetails.Errors.Should().NotBeEmpty();
            }
        }

        private async Task<MeteringPointClient> CreateMeteringPointClient()
        {
            var token = "a3f90ec2-66d1-48fd-8c44-4f78566e3b7b";

            var httpContextAccessorMock = new HttpContextAccessorMock(token);
            var httpClientFactoryMock = new HttpClientFactoryMock(_webApiFactory);
            var meteringPointClientFactory = new MeteringPointClientFactory(httpClientFactoryMock, httpContextAccessorMock);
            var meteringPointClient = meteringPointClientFactory.CreateClient();

            return await Task.FromResult(meteringPointClient).ConfigureAwait(false);
        }
    }
}
