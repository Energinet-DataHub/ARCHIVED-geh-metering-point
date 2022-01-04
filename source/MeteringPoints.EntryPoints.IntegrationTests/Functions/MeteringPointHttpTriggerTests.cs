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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Fixtures;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Functions
{
    [Collection(nameof(IngestionFunctionAppCollectionFixture))]
    public class MeteringPointHttpTriggerTests_RunAsync : FunctionAppTestBase<IngestionFunctionAppFixture>, IAsyncLifetime
    {
        public MeteringPointHttpTriggerTests_RunAsync(IngestionFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        {
            TestFileLoader = new TestFileLoader();
        }

        private TestFileLoader TestFileLoader { get; }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Create_metering_point_request_returns_ok()
        {
            // Arrange
            var xml = TestFileLoader.ReadFile("TestFiles/Cim/CreateMeteringPointConsumption.xml")
                .Replace("{{transactionId}}", "1", StringComparison.OrdinalIgnoreCase)
                .Replace("{{gsrn}}", "571234567891234567", StringComparison.OrdinalIgnoreCase);
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/MeteringPoint");

            request.Headers.Add("Authorization", @"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY3RvcklkIjoiYjE5NTE5NjUtYTQ5ZC00ODBmLTliYWEtYzdhZjJkYjcwMmU0IiwiaWRlbnRpZmllclR5cGUiOiJnbG4iLCJpZGVudGlmaWVyIjoiODIwMDAwMDAwMTQwOSIsInJvbGVzIjpbInNvbWVyb2xlIl19.m-R_MpL7O5HP8bvR6DfVhZt_JvYfS2iIbP4Lk23n8j0");
            request.Content = new StringContent(xml, Encoding.UTF8, "application/xml");

            // Act
            var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request)
                .ConfigureAwait(false);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
