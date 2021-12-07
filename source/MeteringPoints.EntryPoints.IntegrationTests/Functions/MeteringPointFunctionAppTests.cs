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
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Fixtures;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Functions
{
    [Collection(nameof(MeteringPointFunctionAppCollectionFixture))]
    public class MeteringPointFunctionAppTests : IAsyncLifetime
    {
        public MeteringPointFunctionAppTests(MeteringPointFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.SetTestOutputHelper(testOutputHelper);

            TestFileLoader = new TestFileLoader();
        }

        private MeteringPointFunctionAppFixture Fixture { get; }

        private TestFileLoader TestFileLoader { get; }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            Fixture.SetTestOutputHelper(null!);

            return Task.CompletedTask;
        }

        [Fact]
        public async Task Create_metering_point_flow_should_succeed()
        {
            // Arrange
            var xml = TestFileLoader.ReadFile("TestFiles/Cim/CreateMeteringPoint.xml")
                .Replace("{{transactionId}}", "1", StringComparison.OrdinalIgnoreCase)
                .Replace("{{gsrn}}", "571313140733089609", StringComparison.OrdinalIgnoreCase);
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/MeteringPoint");
            request.Headers.Add("gln", "8200000009917");
            request.Content = new StringContent(xml, Encoding.UTF8, "application/xml");

            // Act
            var ingestionResponse = await Fixture.IngestionHostManager.HttpClient.SendAsync(request)
                .ConfigureAwait(false);
            var ingestionResponseBody = await ingestionResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var correlationId = ingestionResponseBody
                .Replace("Correlation id: ", string.Empty, StringComparison.Ordinal)
                .Trim();

            // Assert
            // Ingestion
            ingestionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            Fixture.TestLogger.WriteLine(ingestionResponseBody);

            // Processing
            await AssertFunctionExecuted(Fixture.ProcessingHostManager, "QueueSubscriber").ConfigureAwait(false);

            // Outbox
            await AssertFunctionExecuted(Fixture.OutboxHostManager, "OutboxWatcher").ConfigureAwait(false);

            // MessageHub
            await Fixture.MessageHubSimulator.WaitForNotificationsInDataAvailableQueueAsync(correlationId)
                .ConfigureAwait(false);

            // MessageHub
            // TODO: Check content for accept?
            await Fixture.MessageHubSimulator.PeekAsync().ConfigureAwait(false);

            // Local MessageHub
            await AssertFunctionExecuted(Fixture.LocalMessageHubHostManager, "RequestBundleQueueSubscriber").ConfigureAwait(false);

            // MessageHub
            await Fixture.MessageHubSimulator.DequeueAsync().ConfigureAwait(false);

            // Local MessageHub
            await AssertFunctionExecuted(Fixture.LocalMessageHubHostManager, "BundleDequeuedQueueSubscriber").ConfigureAwait(false);
        }

        private static async Task AssertFunctionExecuted(FunctionAppHostManager hostManager, string functionName)
        {
            var waitTimespan = TimeSpan.FromSeconds(10);

            var functionExecuted = await Awaiter
                .TryWaitUntilConditionAsync(
                    () => hostManager.CheckIfFunctionWasExecuted(
                        $"Functions.{functionName}"),
                    waitTimespan)
                .ConfigureAwait(false);
            functionExecuted.Should().BeTrue($"{functionName} was expected to run.");
        }
    }
}
