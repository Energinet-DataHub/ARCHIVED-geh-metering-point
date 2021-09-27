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
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using NodaTime.Text;
using Squadron;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.IntegrationEvents.ConsumptionMeteringPointCreated
{
    public class ConsumptionMeteringPointCreatedTests : OutboxHost<ConsumptionMeteringPointCreatedServiceBusOptions>
    {
        public ConsumptionMeteringPointCreatedTests(AzureCloudServiceBusResource<ConsumptionMeteringPointCreatedServiceBusOptions> serviceBusResource)
            : base(serviceBusResource)
        {
        }

        [Fact]
        public async Task Dispatch_Consumption_Metering_Point_Created_And_Consume_With_Receiver_And_Assert_Message_Metadata()
        {
                // Arrange
                var outBoxManager = GetService<IOutboxManager>();
                outBoxManager.Add(CreateOutboxMessage());
                var unitOfWork = GetService<IUnitOfWork>();
                await unitOfWork.CommitAsync().ConfigureAwait(false);

                // Act
                var orchestrator = GetService<OutboxOrchestrator>();
                await orchestrator.ProcessOutboxMessagesAsync().ConfigureAwait(false);

                // Get client for consuming events from a Service Bus queue
                var queueClient = GetSubscription(ConsumptionMeteringPointCreatedServiceBusOptions.ServiceBusTopic, ConsumptionMeteringPointCreatedServiceBusOptions.ServiceBusTopicSubscriber);
                var result = await queueClient.AwaitMessageAsync(GetMessage).ConfigureAwait(false);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.UserProperties["EventIdentifier"]);
                result.UserProperties["Timestamp"].Should().Be("2021-09-02T07:11:34Z");
                result.UserProperties["CorrelationId"].Should().Be("00-2f06a6b44f129b4e90a4985a82e77ff5-56e1ec72800dde48-00");
                result.UserProperties["MessageVersion"].Should().Be(1);
                result.UserProperties["MessageType"].Should().Be("ConsumptionMeteringPointCreated");
            }

        private static Task<Message> GetMessage(Message msg, CancellationToken cancellationToken)
        {
            return Task.FromResult(msg);
        }

        private static OutboxMessage CreateOutboxMessage()
        {
            return new(
                "Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Consumption.ConsumptionMeteringPointCreatedIntegrationEvent",
                @"{""MeteringPointId"":""guid"",""GsrnNumber"":""575774240560385464"",""GridAreaCode"":""822"",""SettlementMethod"":""NonProfiled"",""MeteringMethod"":""Physical"",""MeterReadingPeriodicity"":""Hourly"",""NetSettlementGroup"":""Zero"",""ProductType"":""Tariff"",""EffectiveDate"":""2021-09-02T12:10:40Z""}",
                "00-2f06a6b44f129b4e90a4985a82e77ff5-56e1ec72800dde48-00",
                OutboxMessageCategory.IntegrationEvent,
                InstantPattern.General.Parse("2021-09-02T07:11:34Z").Value);
        }
    }
}
