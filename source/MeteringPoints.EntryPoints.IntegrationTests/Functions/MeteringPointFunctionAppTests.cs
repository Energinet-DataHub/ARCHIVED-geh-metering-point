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
using Energinet.DataHub.Core.FunctionApp.TestCommon;
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
        public async Task When_CallingMeteringPoint_Then_RequestIsProcessed()
        {
            // Arrange
            var xml = TestFileLoader.ReadFile("TestFiles/Cim/CreateMeteringPoint.xml")
                .Replace("{{transactionId}}", "1", StringComparison.OrdinalIgnoreCase)
                .Replace("{{gsrn}}", "571234567891234567", StringComparison.OrdinalIgnoreCase);
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/MeteringPoint");
            request.Content = new StringContent(xml, Encoding.UTF8, "application/xml");

            // Act
            var actualResponse = await Fixture.IngestionHostManager.HttpClient.SendAsync(request)
                .ConfigureAwait(false);

            // Assert
            // => Ingestion
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // => Queue subscriber
            var queueSubscriberExecuted = await Awaiter
                .TryWaitUntilConditionAsync(
                    () => Fixture.ProcessingHostManager.CheckIfFunctionWasExecuted("Functions.QueueSubscriber"),
                    TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);

            queueSubscriberExecuted.Should().BeTrue();

            //// TODO: Manually trigger "OutboxWatcher"

            //// TODO: Listen for message in "MESSAGEHUB_QUEUE_DATAAVAILABLE"
        }
    }
}
