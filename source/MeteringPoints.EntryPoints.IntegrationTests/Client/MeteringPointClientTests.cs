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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Client;
using Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Tooling.WebApi;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using FluentAssertions;
using Microsoft.Identity.Client;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Client
{
    [IntegrationTest]
    public class MeteringPointClientTests : WebApiHost
    {
        private readonly WebApiFactory _webApiFactory;

        public MeteringPointClientTests(WebApiFactory webApiFactory, DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _webApiFactory = webApiFactory;
        }

        [Fact(Skip = "Manual test until possible to acquire token from authorization flow")]
        public async Task Get_metering_point_by_gsrn_should_not_be_null()
        {
            // Arrange
            var sut = await CreateMeteringPointClient().ConfigureAwait(false);

            // Act
            var response = await sut.GetMeteringPointByGsrnAsync("571313180400013469").ConfigureAwait(false);

            // Assert
            response.Should().NotBeNull();
            response?.MeteringPointId.Should().Be("e4e88496-1bfd-456d-afcc-3aa9ddd4ef72");
        }

        [Fact(Skip = "Manual test until possible to acquire token from authorization flow")]
        public async Task Get_metering_point_when_not_found_should_be_null()
        {
            // Arrange
            var sut = await CreateMeteringPointClient().ConfigureAwait(false);

            // Act
            var response = await sut.GetMeteringPointByGsrnAsync("foo").ConfigureAwait(false);

            // Assert
            response.Should().BeNull();
        }

        [Fact(Skip = "Manual test until possible to acquire token from authorization flow")]
        public async Task Get_metering_point_without_valid_token_should_throw()
        {
            // Arrange
            var sut = await CreateMeteringPointClient(applyToken: false).ConfigureAwait(false);

            // Act
            Func<Task> response = () => sut.GetMeteringPointByGsrnAsync("foo");

            // Assert
            await response.Should().ThrowAsync<UnauthorizedAccessException>().ConfigureAwait(false);
        }

        private async Task<MeteringPointClient> CreateMeteringPointClient(bool applyToken = true)
        {
            var token = string.Empty;

            if (applyToken)
            {
                var result = await GetTokenAsync().ConfigureAwait(false);
                token = result.AccessToken;
            }

            var httpContextAccessorMock = new HttpContextAccessorMock(token);
            var httpClientFactoryMock = new HttpClientFactoryMock(_webApiFactory);
            var meteringPointClientFactory = new MeteringPointClientFactory(httpClientFactoryMock, httpContextAccessorMock);
            var meteringPointClient = meteringPointClientFactory.CreateClient();

            return meteringPointClient;
        }

        private async Task<AuthenticationResult> GetTokenAsync()
        {
            var confidentialClientApp = CreateConfidentialClientApp();
            return await confidentialClientApp.AcquireTokenForClient(AuthorizationConfiguration.BackendAppScope).ExecuteAsync().ConfigureAwait(false);
        }

        private IConfidentialClientApplication CreateConfidentialClientApp()
        {
            var (teamClientId, teamClientSecret) = AuthorizationConfiguration.ClientCredentialsSettings;

            var confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(teamClientId)
                .WithClientSecret(teamClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{AuthorizationConfiguration.B2cTenantId}"))
                .Build();

            return confidentialClientApp;
        }

        //         private async Task<AuthenticationResult> GetTokenAsync()
//         {
//             var credentials = new NetworkCredential("UserName", "Password");
//             var publicClientApplication = CreatePublicClientApp();
//
//             return await publicClientApplication.AcquireTokenByUsernamePassword(Array.Empty<string>(), credentials.UserName, credentials.SecurePassword).ExecuteAsync().ConfigureAwait(false);
//         }
//
//         private IPublicClientApplication CreatePublicClientApp()
//         {
//             var t = AuthorizationConfiguration.B2cTenantId;
//
//             // var publicClientApplication = PublicClientApplicationBuilder
//             //     .CreateWithApplicationOptions(new PublicClientApplicationOptions
//             //     {
//             //         ClientId = "d91c10bb-1441-4ae5-9bf9-e6845567d018",
//             //         TenantId = "devDataHubB2C.onmicrosoft.com",
//             //     }).Build();
// #pragma warning disable CA2234
//             var publicClientApplication = PublicClientApplicationBuilder
//                 .Create("d91c10bb-1441-4ae5-9bf9-e6845567d018")
//                 .WithB2CAuthority("https://devdatahubb2c.b2clogin.com/tfp/devDataHubB2C.onmicrosoft.com/B2C_1_u001_signin")
// #pragma warning restore CA2234
//                 .Build();
//
//             return publicClientApplication;
//         }
    }
}
