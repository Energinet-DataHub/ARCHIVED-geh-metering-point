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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Charges.Clients.Models;
using Energinet.DataHub.Charges.Clients.SimpleInjector;
using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.MessageHub.Client.SimpleInjector;
using Energinet.DataHub.MeteringPoints.Application.Integrations;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.ActorMessages;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.Common;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.Functions;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.IntegrationEventDispatchers;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.MessageHub;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MessageHub;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.Connect;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Consumption;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Exchange;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.MessageDequeued;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Production;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.Disconnect;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.Reconnect;
using Energinet.DataHub.MeteringPoints.Infrastructure.LocalMessageHub;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Energinet.DataHub.MeteringPoints.Messaging;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

[assembly: CLSCompliant(false)]

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Outbox
{
    public class Program : EntryPoint
    {
        public static async Task Main()
        {
            var program = new Program();

            var host = program.ConfigureApplication();
            program.AssertConfiguration();
            await program.ExecuteApplicationAsync(host).ConfigureAwait(false);
        }

        protected override void ConfigureServiceCollection(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            base.ConfigureServiceCollection(services);

            services.AddDbContext<MeteringPointContext>(x =>
            {
                var connectionString = Environment.GetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING")
                                       ?? throw new InvalidOperationException(
                                           "Metering point db connection string not found.");

                x.UseSqlServer(connectionString, y => y.UseNodaTime());
            });
        }

        protected override void ConfigureContainer(Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            base.ConfigureContainer(container);

            // Register application components.
            container.Register<ISystemDateTimeProvider, SystemDateTimeProvider>(Lifestyle.Scoped);
            container.Register<IJsonSerializer, JsonSerializer>(Lifestyle.Scoped);
            container.Register<IOutboxManager, OutboxManager>(Lifestyle.Scoped);
            container.Register<IUnitOfWork, UnitOfWork>(Lifestyle.Scoped);
            container.Register<OutboxWatcher>(Lifestyle.Scoped);
            container.Register<OutboxOrchestrator>(Lifestyle.Scoped);
            container.Register<IOutboxMessageDispatcher, OutboxMessageDispatcher>(Lifestyle.Scoped);
            container.RegisterDecorator<IOutboxMessageDispatcher, OutboxMessageDispatcherTelemetryDecorator>(Lifestyle.Scoped);
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Scoped);
            container.Register<IIntegrationMetadataContext, IntegrationMetadataContext>(Lifestyle.Scoped);
            container.Register<IIntegrationEventMessageFactory, IntegrationEventServiceBusMessageFactory>(Lifestyle.Scoped);

            var connectionString =
                Environment.GetEnvironmentVariable("SHARED_INTEGRATION_EVENT_SERVICE_BUS_SENDER_CONNECTION_STRING");
            container.Register<ServiceBusClient>(
                () => new ServiceBusClient(connectionString),
                Lifestyle.Singleton);

            container.Register(
                () => new MeteringPointCreatedTopic(
                    Environment.GetEnvironmentVariable("METERING_POINT_CREATED_TOPIC") ??
                    throw new InvalidOperationException(
                        "No MeteringPointCreated Topic found")),
                Lifestyle.Singleton);
            container.Register(
                () => new ConsumptionMeteringPointCreatedTopic(
                    Environment.GetEnvironmentVariable("CONSUMPTION_METERING_POINT_CREATED_TOPIC") ??
                    throw new InvalidOperationException(
                        "No Consumption Metering Point Created Topic found")),
                Lifestyle.Singleton);
            container.Register(
                () => new ProductionMeteringPointCreatedTopic(
                    Environment.GetEnvironmentVariable("PRODUCTION_METERING_POINT_CREATED_TOPIC") ??
                    throw new InvalidOperationException(
                        "No Production Metering Point Created Topic found")),
                Lifestyle.Singleton);

            container.Register(
                () => new ExchangeMeteringPointCreatedTopic(
                    Environment.GetEnvironmentVariable("EXCHANGE_METERING_POINT_CREATED_TOPIC") ??
                    throw new InvalidOperationException(
                        "No Exchange Metering Point Created Topic found")),
                Lifestyle.Singleton);

            container.Register(
                () => new MeteringPointConnectedTopic(
                    Environment.GetEnvironmentVariable("METERING_POINT_CONNECTED_TOPIC") ??
                    throw new InvalidOperationException(
                        "No MeteringPointConnected Topic found")),
                Lifestyle.Singleton);
            container.Register(
                () => new MeteringPointMessageDequeuedTopic(
                    Environment.GetEnvironmentVariable("METERING_POINT_MESSAGE_DEQUEUED_TOPIC") ??
                    throw new InvalidOperationException(
                        "No MeteringPointConnected Topic found")),
                Lifestyle.Singleton);

            container.Register(
                () => new MeteringPointDisconnectedTopic(
                    Environment.GetEnvironmentVariable("METERING_POINT_DISCONNECTED_TOPIC") ??
                    throw new InvalidOperationException(
                        "No MeteringPointDisconnected Topic found")),
                Lifestyle.Singleton);

            container.Register(
                () => new MeteringPointReconnectedTopic(
                    Environment.GetEnvironmentVariable("METERING_POINT_RECONNECTED_TOPIC") ??
                    throw new InvalidOperationException(
                        "No MeteringPointReconnected Topic found")),
                Lifestyle.Singleton);

            container.Register(typeof(ITopicSender<>), typeof(TopicSender<>), Lifestyle.Singleton);

            container.SendProtobuf<IntegrationEventEnvelope>();

            container.Register<IMessageHubMessageRepository, MessageHubMessageRepository>(Lifestyle.Scoped);
            container.Register<ILocalMessageHubDataAvailableClient, LocalMessageHubDataAvailableClient>(Lifestyle.Scoped);

            container.Register<IOutboxDispatcher<DataAvailableNotification>, DataAvailableNotificationOutboxDispatcher>();
            container.Register<IOutboxMessageFactory, OutboxMessageFactory>(Lifestyle.Scoped);
            container.Register<IOutbox, OutboxProvider>(Lifestyle.Scoped);

            container.Register<MessageHubMessageFactory>(Lifestyle.Scoped);

            var messageHubStorageConnectionString = Environment.GetEnvironmentVariable("MESSAGEHUB_STORAGE_CONNECTION_STRING") ?? throw new InvalidOperationException("MessageHub storage connection string not found.");
            var messageHubStorageContainerName = Environment.GetEnvironmentVariable("MESSAGEHUB_STORAGE_CONTAINER_NAME") ?? throw new InvalidOperationException("MessageHub storage container name not found.");
            var messageHubServiceBusConnectionString = Environment.GetEnvironmentVariable("MESSAGEHUB_QUEUE_CONNECTION_STRING") ?? throw new InvalidOperationException("MessageHub queue connection string not found.");
            var messageHubServiceBusDataAvailableQueue = Environment.GetEnvironmentVariable("MESSAGEHUB_DATA_AVAILABLE_QUEUE") ?? throw new InvalidOperationException("MessageHub data available queue not found.");
            var messageHubServiceBusDomainReplyQueue = Environment.GetEnvironmentVariable("MESSAGEHUB_DOMAIN_REPLY_QUEUE") ?? throw new InvalidOperationException("MessageHub reply queue not found.");

            container.AddMessageHubCommunication(
                messageHubServiceBusConnectionString,
                new MessageHubConfig(messageHubServiceBusDataAvailableQueue, messageHubServiceBusDomainReplyQueue),
                messageHubStorageConnectionString,
                new StorageConfig(messageHubStorageContainerName));

            container.AddDefaultChargeLinkClient(
                container.GetInstance<ServiceBusClient>,
                new ServiceBusRequestSenderConfiguration(Environment.GetEnvironmentVariable("CHARGES_DEFAULT_LINK_RESPONSE_QUEUE") ?? throw new InvalidOperationException()));

            container.UseMediatR()
                .WithPipeline()
                .WithRequestHandlers(
                    typeof(DataAvailableNotificationDispatcher),
                    typeof(DataBundleResponseDispatcher),
                    typeof(ConsumptionMeteringPointCreatedDispatcher),
                    typeof(CreateMeteringPointDispatcher),
                    typeof(ExchangeMeteringPointCreatedDispatcher),
                    typeof(MeteringPointConnectedDispatcher),
                    typeof(MeteringPointDisconnectedDispatcher),
                    typeof(MeteringPointMessageDequeuedIntegrationEventDispatcher),
                    typeof(MeteringPointReconnectedDispatcher),
                    typeof(NullMeteringConfigrationChangedDispatcher),
                    typeof(ProductionMeteringPointCreatedDispatcher),
                    typeof(RequestDefaultChargeLinksDispatcher),
                    typeof(MessageHubDispatcher));
        }
    }
}
