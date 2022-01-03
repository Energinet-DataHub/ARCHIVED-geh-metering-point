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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Extensions;
using Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
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

        [Theory]
        [InlineData("TestFiles/Cim/CreateMeteringPointConsumption.xml")]
        public async Task Create_metering_point_flow_should_succeed(string testFileXml)
        {
            // Arrange
            var xml = TestFileLoader.ReadFile(testFileXml)
                .Replace("{{transactionId}}", "1", StringComparison.OrdinalIgnoreCase)
                .Replace("{{gsrn}}", TestDataCreator.CreateGsrn(), StringComparison.OrdinalIgnoreCase);
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/MeteringPoint");

            request.Headers.Add("Authorization", @"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY3RvcklkIjoiYjE5NTE5NjUtYTQ5ZC00ODBmLTliYWEtYzdhZjJkYjcwMmU0IiwiaWRlbnRpZmllclR5cGUiOiJnbG4iLCJpZGVudGlmaWVyIjoiODIwMDAwMDAwNTcxMSIsInJvbGVzIjpbInNvbWVyb2xlIl19.HfhMutQgCt6hbrFjz_sT3Bsx06W6WybDMGV68omqKa0");
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
            await Fixture.MessageHubSimulator
                .WaitForNotificationsInDataAvailableQueueAsync(correlationId)
                .ConfigureAwait(false);

            var peekSimulationResponseDto = await Fixture.MessageHubSimulator.PeekAsync().ConfigureAwait(false);

            // Local MessageHub
            await AssertFunctionExecuted(Fixture.LocalMessageHubHostManager, "RequestBundleQueueSubscriber").ConfigureAwait(false);

            // MessageHub
            await Fixture.MessageHubSimulator.DequeueAsync(peekSimulationResponseDto).ConfigureAwait(false);

            // Local MessageHub
            await AssertFunctionExecuted(Fixture.LocalMessageHubHostManager, "BundleDequeuedQueueSubscriber").ConfigureAwait(false);

            AssertConfirmMessages();
            AssertNoExceptionsThrown();
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

        private void AssertConfirmMessages()
        {
            using (new AssertionScope())
            {
                foreach (var notificationDto in Fixture.MessageHubSimulator.Notifications)
                {
                    // TODO: Do actual check for confirmed or maybe put the expected type in the InlineData.
                    notificationDto.MessageType.Value.Should().StartWith("Confirm");
                }

                // Clear available messages in simulator between runs.
                Fixture.MessageHubSimulator.Clear();
            }
        }

        private void AssertNoExceptionsThrown()
        {
            Fixture.IngestionHostManager.CheckIfFunctionThrewException().Should().BeFalse();
            Fixture.ProcessingHostManager.CheckIfFunctionThrewException().Should().BeFalse();
            Fixture.OutboxHostManager.CheckIfFunctionThrewException().Should().BeFalse();
            Fixture.LocalMessageHubHostManager.CheckIfFunctionThrewException().Should().BeFalse();
            Fixture.InternalCommandDispatcherHostManager.CheckIfFunctionThrewException().Should().BeFalse();
        }
    }
}
