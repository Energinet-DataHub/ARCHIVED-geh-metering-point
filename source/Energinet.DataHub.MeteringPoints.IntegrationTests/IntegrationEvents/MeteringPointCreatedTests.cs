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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using NodaTime.Text;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Squadron;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.IntegrationEvents
{
    [Trait("Category", "Integration")]
    public class MeteringPointCreatedTests : IClassFixture<AzureCloudServiceBusResource<MeteringPointCreatedServiceBusOptions>>
    {
        private readonly AzureCloudServiceBusResource<MeteringPointCreatedServiceBusOptions> _serviceBusResource;

        public MeteringPointCreatedTests(AzureCloudServiceBusResource<MeteringPointCreatedServiceBusOptions> serviceBusResource)
        {
            _serviceBusResource = serviceBusResource;
        }

        [Fact]
        public async Task DispatchMeteringPointCreatedAndConsumeWithReceiverAndAssertMessageContent()
        {
            // Setup
            await using var sendingContainer = new Container();
            ContainerHelper.CreateContainer(sendingContainer);
            ContainerHelper.RegisterServiceBusService(sendingContainer, _serviceBusResource);
            ContainerHelper.VerifyContainer(sendingContainer);

            await using (AsyncScopedLifestyle.BeginScope(sendingContainer))
            {
                // Arrange
                var outBoxManager = sendingContainer.GetRequiredService<IOutboxManager>();
                outBoxManager.Add(CreateOutboxMessage());
                var unitOfWork = sendingContainer.GetRequiredService<IUnitOfWork>();
                await unitOfWork.CommitAsync().ConfigureAwait(false);

                // Act
                var orchestrator = sendingContainer.GetRequiredService<OutboxOrchestrator>();
                await orchestrator.ProcessOutboxMessagesAsync().ConfigureAwait(false);

                // Get client for consuming events from a Service Bus queue
                var queueClient = _serviceBusResource.GetSubscriptionClient(MeteringPointCreatedServiceBusOptions.ServiceBusTopic, MeteringPointCreatedServiceBusOptions.ServiceBusTopicSubscriber);
                var result = await queueClient.AwaitMessageAsync(GetMessage).ConfigureAwait(false);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.UserProperties["EventIdentifier"]);
                result.UserProperties["Timestamp"].Should().Be("2021-09-02T07:11:34Z");
                result.UserProperties["CorrelationId"].Should().Be("00-2f06a6b44f129b4e90a4985a82e77ff5-56e1ec72800dde48-00");
                result.UserProperties["MessageVersion"].Should().Be(1);
                result.UserProperties["MessageType"].Should().Be("MeteringPointCreated");

                ContainerHelper.CleanupDatabase(sendingContainer);
            }
        }

        private static Task<Message> GetMessage(Message msg, CancellationToken cancellationToken)
        {
            return Task.FromResult(msg);
        }

        private static OutboxMessage CreateOutboxMessage()
        {
            return new(
                "Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.MeteringPointCreatedEventMessage",
                @"{""Gsrn"":""574591757409421563"",""MeteringPointType"":""Consumption"",""GridAreaId"":""a64278ed-1fa2-4d34-bb50-161f04f09eb0"",""SettlementMethod"":""NonProfiled"",""MeteringMethod"":""Physical"",""ConnectionState"":""New"",""MeterReadingPeriodicity"":""Hourly"",""NetSettlementGroup"":""Zero"",""ToGrid"":"""",""FromGrid"":"""",""Product"":""Tariff"",""QuantityUnit"":""KWh"",""ParentGsrn"":"""",""EffectiveDate"":""2021-09-02T07:11:34Z""}",
                "00-2f06a6b44f129b4e90a4985a82e77ff5-56e1ec72800dde48-00",
                OutboxMessageCategory.IntegrationEvent,
                InstantPattern.General.Parse("2021-09-02T07:11:34Z").Value);
        }
    }
}
