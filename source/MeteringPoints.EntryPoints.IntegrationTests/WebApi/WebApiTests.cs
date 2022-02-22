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

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Tooling.WebApi;
using Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Tooling.WebApi.Hosts;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.WebApi
{
    [IntegrationTest]
    public class WebApiTests : WebApiHostWithoutSecurity
    {
        private readonly WebApiWithoutSecurityFactory _webApiFactory;

        public WebApiTests(WebApiWithoutSecurityFactory webApiFactory, DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _webApiFactory = webApiFactory;
        }

        [Fact]
        public async Task Get_metering_point_dates_must_include_utc_identifier()
        {
            // Arrange
            var httpClient = _webApiFactory.CreateClient();
            var userId = "a3f90ec2-66d1-48fd-8c44-4f78566e3b7b";
            var gsrn = "571313180400013469";

            // Act
            var response = await GetMeteringPoint(httpClient, userId, gsrn).ConfigureAwait(false);

            // Assert
            var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var document = JsonDocument.Parse(jsonString);
            var root = document.RootElement;
            var effectiveDateElement = root.GetProperty("effectiveDate").GetString();
            effectiveDateElement.Should().EndWith("Z");
        }

        [Fact]
        public async Task Get_metering_point_process_dates_must_include_utc_identifier()
        {
            // Arrange
            var httpClient = _webApiFactory.CreateClient();
            var userId = "a3f90ec2-66d1-48fd-8c44-4f78566e3b7b";
            var gsrn = "571313157178361184";

            // Act
            var response = await GetMeteringPointProcesses(httpClient, userId, gsrn).ConfigureAwait(false);

            // Assert
            var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var document = JsonDocument.Parse(jsonString);
            var root = document.RootElement;
            var effectiveDateElement = root.EnumerateArray().First().GetProperty("effectiveDate").GetString();
            effectiveDateElement.Should().EndWith("Z");
        }

        private static async Task<HttpResponseMessage> GetMeteringPoint(HttpClient httpClient, string userId, string gsrn)
        {
            return await GetMethod(httpClient, userId, gsrn, "GetMeteringPointByGsrn").ConfigureAwait(false);
        }

        private static async Task<HttpResponseMessage> GetMeteringPointProcesses(HttpClient httpClient, string userId, string gsrn)
        {
            return await GetMethod(httpClient, userId, gsrn, "GetMeteringPointProcessesByGsrn").ConfigureAwait(false);
        }

        private static async Task<HttpResponseMessage> GetMethod(HttpClient httpClient, string userId, string gsrn, string method)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", userId);
            var response = await httpClient
                .GetAsync(new Uri($"MeteringPoint/{method}/?gsrn={gsrn}", UriKind.Relative))
                .ConfigureAwait(false);
            return response;
        }
    }
}
