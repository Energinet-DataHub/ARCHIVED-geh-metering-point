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
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
            var serviceCollection = new ServiceCollection();
            // Send setup
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

            // Arrange

            // Act
            await using (AsyncScopedLifestyle.BeginScope(sendingContainer))
            {
                var orchestrator = sendingContainer.GetRequiredService<OutboxOrchestrator>();
                await orchestrator.ProcessOutboxMessagesAsync().ConfigureAwait(false);

                // Get client for consuming events from a Service Bus queue
                var queueClient = _serviceBusResource.GetSubscriptionClient(
                    "metering-point-created",
                    MeteringPointCreatedServicebusOptions.ServiceBusTopicSubscriber);

                var result = await queueClient.AwaitMessageAsync(GetMessage, TimeSpan.FromSeconds(5)).ConfigureAwait(false);

                var completion = new TaskCompletionSource<ServiceBusMessage>();
                RegisterSubscriberMessageHandler(queueClient, completion);

                // Assert
                ServiceBusMessage message = await completion.Task.ConfigureAwait(false);
            }

            // await using var receivingContainer = new Container();
            // receivingContainer.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            // receivingContainer.Verify();
        }

        private static Task<Message> GetMessage(Message msg, CancellationToken cancellationToken)
        {
            return Task.FromResult(msg);
        }

        private void RegisterSubscriberMessageHandler<T>(IReceiverClient queueClient, TaskCompletionSource<T> completion)
        {
            queueClient.RegisterMessageHandler(
                (message, _) =>
            {
                try
                {
                    var eventDataString = Encoding.UTF8.GetString(message.Body);
                    var received = JsonConvert.DeserializeObject<T>(eventDataString);
                    completion.SetResult(received);
                }
                catch (Exception exception)
                {
                    completion.SetException(exception);
                    throw new InvalidOperationException(exception.Message);
                }

                return Task.CompletedTask;
            },
                new MessageHandlerOptions(ExceptionReceivedHandler));
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            throw new NotImplementedException();
        }
    }
}
