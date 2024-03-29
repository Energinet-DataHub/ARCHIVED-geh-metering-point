﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Core.App.Common;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.MessageHub.Client.SimpleInjector;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.LocalMessageHub.Functions;
using Energinet.DataHub.MeteringPoints.EntryPoints.LocalMessageHub.Monitor;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.Configuration;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MessageHub;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MessageHub.Bundling;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.GenericNotification;
using Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion;
using Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion.Mappers;
using Energinet.DataHub.MeteringPoints.Infrastructure.LocalMessageHub;
using Energinet.DataHub.MeteringPoints.Infrastructure.Messaging.Idempotency;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.Messaging;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Confirm;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Generic;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Reject;
using Energinet.DataHub.MeteringPoints.RequestResponse.Contract;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.LocalMessageHub
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

        protected override void ConfigureFunctionsWorkerDefaults(IFunctionsWorkerApplicationBuilder options)
        {
            base.ConfigureFunctionsWorkerDefaults(options);

            options.UseMiddleware<CorrelationIdMiddleware>();
            options.UseMiddleware<ServiceBusSessionIdMiddleware>();
            options.UseMiddleware<EntryPointTelemetryScopeMiddleware>();
        }

        protected override void ConfigureServiceCollection(IServiceCollection services)
        {
            base.ConfigureServiceCollection(services);

            services.AddDbContext<MeteringPointContext>(x =>
            {
                var connectionString = Environment.GetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING")
                                       ?? throw new InvalidOperationException(
                                           "Metering point db connection string not found.");

                x.UseSqlServer(connectionString, y => y.UseNodaTime());
            });

            // Health Checks
            services.AddLiveHealthCheck();
            services.AddSqlServerHealthCheck(Environment.GetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING")!);
            services.AddExternalServiceBusQueuesHealthCheck(
                Environment.GetEnvironmentVariable("SHARED_SERVICE_BUS_MANAGE_CONNECTION_STRING")!,
                Environment.GetEnvironmentVariable("MESSAGEHUB_DATA_AVAILABLE_QUEUE")!,
                Environment.GetEnvironmentVariable("MESSAGEHUB_DOMAIN_REPLY_QUEUE")!,
                Environment.GetEnvironmentVariable("REQUEST_BUNDLE_QUEUE_SUBSCRIBER_QUEUE")!,
                Environment.GetEnvironmentVariable("BUNDLE_DEQUEUED_SUBSCRIBER_QUEUE")!,
                Environment.GetEnvironmentVariable("METERINGPOINT_QUEUE_NAME")!);
        }

        protected override void ConfigureContainer(Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            base.ConfigureContainer(container);

            // Register application components.
            container.Register<HealthCheckEndpoint>(Lifestyle.Scoped);
            container.Register<RequestBundleQueueSubscriber>(Lifestyle.Scoped);
            container.Register<BundleDequeuedQueueSubscriber>(Lifestyle.Scoped);
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Scoped);
            container.Register<CorrelationIdMiddleware>(Lifestyle.Scoped);
            container.Register<ISessionContext, SessionContext>(Lifestyle.Scoped);
            container.Register<ServiceBusSessionIdMiddleware>(Lifestyle.Scoped);
            container.Register<EntryPointTelemetryScopeMiddleware>(Lifestyle.Scoped);
            container.Register<IUnitOfWork, UnitOfWork>();
            container.Register<IJsonSerializer, JsonSerializer>(Lifestyle.Singleton);
            container.Register<ISystemDateTimeProvider, SystemDateTimeProvider>(Lifestyle.Singleton);
            container.Register<IIncomingMessageRegistry, IncomingMessageRegistry>(Lifestyle.Transient);
            container.SendProtobuf<MeteringPointEnvelope>();
            container.Register<IMessageDispatcher, InternalDispatcher>(Lifestyle.Scoped);
            container.Register<Channel, InternalServiceBus>(Lifestyle.Scoped);
            container.Register<IActorContext, ActorContext>(Lifestyle.Scoped);
            container.Register(typeof(ProtobufOutboundMapper<>), typeof(ProtobufOutboundMapper<>).Assembly);
            container.Register<ProtobufOutboundMapperFactory>();
            container.Register<ProtobufInboundMapperFactory>();

            container.UseMediatR()
                .WithPipeline()
                .WithRequestHandlers(
                    typeof(ConfirmMessageBundleHandler),
                    typeof(RejectMessageBundleHandler),
                    typeof(GenericNotificationBundleHandler),
                    typeof(AccountingPointCharacteristicsBundleHandler));

            var messageHubStorageConnectionString = Environment.GetEnvironmentVariable("MESSAGEHUB_STORAGE_CONNECTION_STRING") ?? throw new InvalidOperationException("MessageHub storage connection string not found.");
            var messageHubStorageContainerName = Environment.GetEnvironmentVariable("MESSAGEHUB_STORAGE_CONTAINER_NAME") ?? throw new InvalidOperationException("MessageHub storage container name not found.");
            var messageHubServiceBusConnectionString = Environment.GetEnvironmentVariable("MESSAGEHUB_QUEUE_CONNECTION_STRING") ?? throw new InvalidOperationException("MessageHub queue connection string not found.");

            container.AddMessageHubCommunication(
                messageHubServiceBusConnectionString,
                new MessageHubConfig(
                    Environment.GetEnvironmentVariable("MESSAGEHUB_DATA_AVAILABLE_QUEUE") ?? throw new InvalidOperationException("MessageHub data available queue not found."),
                    Environment.GetEnvironmentVariable("MESSAGEHUB_DOMAIN_REPLY_QUEUE") ?? throw new InvalidOperationException("MessageHub domain reply queue not found.")),
                messageHubStorageConnectionString,
                new StorageConfig(messageHubStorageContainerName));

            container.Register<ILocalMessageHubClient, LocalMessageHubClient>(Lifestyle.Scoped);
            container.Register<IMessageHubMessageRepository, MessageHubMessageRepository>(Lifestyle.Scoped);
            container.Register<IOutboxDispatcher<DataBundleResponse>, DataBundleResponseOutboxDispatcher>();
            container.Register<IOutboxDispatcher<MessageHubMessage>, MeteringPointMessageDequeuedIntegrationEventOutboxDispatcher>(Lifestyle.Scoped);
            container.Register<IOutboxMessageFactory, OutboxMessageFactory>(Lifestyle.Scoped);
            container.Register<IOutbox, OutboxProvider>(Lifestyle.Scoped);
            container.Register<IBundleCreator, BundleCreator>(Lifestyle.Scoped);
            container.Register<IDocumentSerializer<ConfirmMessage>, ConfirmMessageXmlSerializer>(Lifestyle.Singleton);
            container.Register<IDocumentSerializer<RejectMessage>, RejectMessageXmlSerializer>(Lifestyle.Singleton);
            container.Register<IDocumentSerializer<GenericNotificationMessage>, GenericNotificationMessageXmlSerializer>(Lifestyle.Singleton);
            container.Register<IDocumentSerializer<AccountingPointCharacteristicsMessage>, AccountingPointCharacteristicsMessageXmlSerializer>(Lifestyle.Singleton);

            var connectionString = Environment.GetEnvironmentVariable("SHARED_SERVICE_BUS_SENDER_CONNECTION_STRING");
            var topic = Environment.GetEnvironmentVariable("METERINGPOINT_QUEUE_NAME");

            container.Register(() => new ServiceBusClient(connectionString).CreateSender(topic), Lifestyle.Singleton);
        }
    }
}
