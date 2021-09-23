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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.PostOffice;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Text;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Squadron;
using Xunit;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.IntegrationEvents
{
    [Trait("Category", "Integration")]
    public class MeteringPointCreatedTests
    : IClassFixture<AzureCloudServiceBusResource<MeteringPointCreatedServicebusOptions>>
    {
        private readonly AzureCloudServiceBusResource<MeteringPointCreatedServicebusOptions> _serviceBusResource;

        public MeteringPointCreatedTests(AzureCloudServiceBusResource<MeteringPointCreatedServicebusOptions> serviceBusResource)
        {
            _serviceBusResource = serviceBusResource;
        }

        [Fact]
        public async Task DispatchMeteringPointCreatedAndConsumeWithReceiverAndAssertMessageContent()
        {
            // Setup
            var serviceCollection = new ServiceCollection();
            await using var sendingContainer = new Container();
            sendingContainer.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            serviceCollection.AddDbContext<MeteringPointContext>(x =>
            {
                var connectionString = "Server=localhost;Database=MeteringPointTestDB;Trusted_Connection=True;";

                x.UseSqlServer(connectionString, y => y.UseNodaTime());
            });

            serviceCollection.AddSimpleInjector(sendingContainer);
            serviceCollection.BuildServiceProvider().UseSimpleInjector(sendingContainer);
            sendingContainer.Register(
                () => new PostOfficeStorageClientSettings(
                    "DefaultEndpointsProtocol=https;AccountName=stormeteringpointtmpu;AccountKey=KwFnZJh3Tv/am6o8SdPeA/GplYwitkCsFt6GajCpRY1zoRkdyCrpfASWegYDYRlI+saRBY4ecL4+27D4sTFoQA==;EndpointSuffix=core.windows.net",
                    "temppostoffice"));
            sendingContainer.Register<IOutboxMessageDispatcher, OutboxMessageDispatcher>(Lifestyle.Scoped);
            sendingContainer.Register<IUnitOfWork, UnitOfWork>(Lifestyle.Scoped);
            sendingContainer.Register<ISystemDateTimeProvider, SystemDateTimeProviderStub>(Lifestyle.Scoped);
            sendingContainer.Register<IOutboxManager, OutboxManager>(Lifestyle.Scoped);
            sendingContainer.Register<OutboxOrchestrator>(Lifestyle.Scoped);
            sendingContainer.Register<IPostOfficeStorageClient, TempPostOfficeStorageClient>(Lifestyle.Scoped);
            sendingContainer.Register<IIntegrationMetaDataContext, IntegrationMetaDataContext>(Lifestyle.Scoped);
            sendingContainer.Register<IJsonSerializer, Energinet.DataHub.MeteringPoints.Infrastructure.Serialization.JsonSerializer>(Lifestyle.Scoped);
            sendingContainer.Register<ServiceBusClient>(
                () => new ServiceBusClient(_serviceBusResource.ConnectionString),
                Lifestyle.Singleton);
            sendingContainer.Register(
                () => new MeteringPointCreatedTopic("metering-point-created"),
                Lifestyle.Singleton);
            sendingContainer.Register(
                () => new MeteringPointConnectedTopic("metering-point-connected"),
                Lifestyle.Singleton);
            sendingContainer.Register(typeof(ITopicSender<>), typeof(TopicSender<>), Lifestyle.Singleton);
            sendingContainer.SendProtobuf<IntegrationEventEnvelope>();
            sendingContainer.BuildMediator(
                new[]
                {
                    typeof(OutboxWatcher).Assembly,
                },
                Array.Empty<Type>());
            sendingContainer.Verify();

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
                var queueClient = _serviceBusResource.GetSubscriptionClient(
                    "metering-point-created",
                    MeteringPointCreatedServicebusOptions.ServiceBusTopicSubscriber);

                var result = await queueClient.AwaitMessageAsync(GetMessage).ConfigureAwait(false);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.UserProperties["EventIdentifier"]);
                result.UserProperties["Timestamp"].Should().Be("2021-09-02T07:11:34Z");
                result.UserProperties["CorrelationId"].Should().Be("00-2f06a6b44f129b4e90a4985a82e77ff5-56e1ec72800dde48-00");
                result.UserProperties["MessageVersion"].Should().Be(1);
                result.UserProperties["MessageType"].Should().Be("MeteringPointCreated");
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
