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
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.ChangeConnectionStatus;
using Energinet.DataHub.MeteringPoints.Application.CloseDown;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.ChildMeteringPoints;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Application.Common.DomainEvents;
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.Create.Validation;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Application.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Application.Integrations.ChargeLinks.Create;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Application.ProcessOverview;
using Energinet.DataHub.MeteringPoints.Application.Providers;
using Energinet.DataHub.MeteringPoints.Application.Providers.MeteringPointOwnership;
using Energinet.DataHub.MeteringPoints.Application.RequestMasterData;
using Energinet.DataHub.MeteringPoints.Application.UpdateMasterData;
using Energinet.DataHub.MeteringPoints.Application.Validation;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses.UpdateMasterData;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.Processing.Functions;
using Energinet.DataHub.MeteringPoints.EntryPoints.Processing.Functions.EventListeners;
using Energinet.DataHub.MeteringPoints.EntryPoints.Processing.Monitor;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Authorization;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Pipeline;
using Energinet.DataHub.MeteringPoints.Infrastructure.Configuration;
using Energinet.DataHub.MeteringPoints.Infrastructure.ContainerExtensions;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.EnergySuppliers.Queries;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.GridAreas;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MessageHub.Bundling;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints.Queries;
using Energinet.DataHub.MeteringPoints.Infrastructure.DomainEventDispatching;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ChangeConnectionStatus;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ChangeMasterData;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ConnectMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.GenericNotification;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.ChargeLinks.Create;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeConnectionStatus.Disconnect;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeConnectionStatus.Reconnect;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeMasterData.MasterDataUpdated;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.Connect;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Notifications;
using Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands;
using Energinet.DataHub.MeteringPoints.Infrastructure.Messaging.Idempotency;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Providers;
using Energinet.DataHub.MeteringPoints.Infrastructure.Providers.MeteringPointOwnership;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Confirm;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Generic;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Reject;
using Energinet.DataHub.MeteringPoints.RequestResponse.Contract;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector;
using MasterDataDocument = Energinet.DataHub.MeteringPoints.Application.MarketDocuments.MasterDataDocument;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
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
            options.UseMiddleware<EntryPointTelemetryScopeMiddleware>();
            options.UseMiddleware<ServiceBusActorContextMiddleware>();
            // TODO: Fix the duplicate check for ingestion messages and re-enable this https://app.zenhub.com/workspaces/batman-60a6105157304f00119be86e/issues/energinet-datahub/geh-metering-point/378
            // options.UseMiddleware<ServiceBusMessageIdempotencyMiddleware>();
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
                Environment.GetEnvironmentVariable("MASTER_DATA_REQUEST_QUEUE_NAME")!,
                "metering-point-master-data-response",
                Environment.GetEnvironmentVariable("METERINGPOINT_QUEUE_NAME")!);
            services.AddExternalServiceBusTopicsHealthCheck(
                Environment.GetEnvironmentVariable("SHARED_SERVICE_BUS_MANAGE_CONNECTION_STRING")!,
                Environment.GetEnvironmentVariable("INTEGRATION_EVENT_TOPIC_NAME")!);
            services.AddExternalServiceBusSubscriptionsHealthCheck(
                Environment.GetEnvironmentVariable("SHARED_SERVICE_BUS_MANAGE_CONNECTION_STRING")!,
                Environment.GetEnvironmentVariable("INTEGRATION_EVENT_TOPIC_NAME")!,
                Environment.GetEnvironmentVariable("MARKET_PARTICIPANT_CHANGED_ACTOR_CREATED_SUBSCRIPTION_NAME")!,
                Environment.GetEnvironmentVariable("MARKET_PARTICIPANT_CHANGED_ACTOR_ROLE_ADDED_SUBSCRIPTION_NAME")!,
                Environment.GetEnvironmentVariable("MARKET_PARTICIPANT_CHANGED_ACTOR_ROLE_REMOVED_SUBSCRIPTION_NAME")!,
                Environment.GetEnvironmentVariable("MARKET_PARTICIPANT_CHANGED_ACTOR_GRID_AREA_ADDED_SUBSCRIPTION_NAME")!,
                Environment.GetEnvironmentVariable("MARKET_PARTICIPANT_CHANGED_ACTOR_GRID_AREA_REMOVED_SUBSCRIPTION_NAME")!,
                Environment.GetEnvironmentVariable("MARKET_PARTICIPANT_CHANGED_GRID_AREA_CREATED_SUBSCRIPTION_NAME")!,
                Environment.GetEnvironmentVariable("MARKET_PARTICIPANT_CHANGED_GRID_AREA_NAME_CHANGED_SUBSCRIPTION_NAME")!,
                Environment.GetEnvironmentVariable("ENERGY_SUPPLIER_CHANGED_EVENT_SUBSCRIPTION_NAME")!);
        }

        protected override void ConfigureContainer(Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            base.ConfigureContainer(container);

            // Register application components.
            container.Register<IActorLookup, ActorLookup>(Lifestyle.Scoped);
            container.Register<QueueSubscriber>(Lifestyle.Scoped);
            container.Register<SystemTimer>(Lifestyle.Scoped);
            container.Register<InternalCommandProcessor>(Lifestyle.Scoped);
            container.Register<InternalCommandAccessor>(Lifestyle.Scoped);
            container.Register<CommandExecutor>(Lifestyle.Scoped);

            var connectionString = Environment.GetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING")
                                   ?? throw new InvalidOperationException(
                                       "Metering point db connection string not found.");
            container.Register<IDbConnectionFactory>(() => new SqlDbConnectionFactory(connectionString), Lifestyle.Scoped);
            container.Register<DbGridAreaHelper>(Lifestyle.Scoped);
            container.Register<ChargesResponseReceiver>(Lifestyle.Scoped);
            container.Register<MasterDataRequestListener>(Lifestyle.Scoped);
            container.Register<EnergySupplierChangedListener>(Lifestyle.Scoped);
            container.Register<MeteringPointMessageDequeuedListener>(Lifestyle.Scoped);
            container.Register<IMeteringPointRepository, MeteringPointRepository>(Lifestyle.Scoped);
            container.Register<IEnergySupplierRepository, EnergySupplierRepository>(Lifestyle.Scoped);
            container.Register<IGridAreaRepository, GridAreaRepository>(Lifestyle.Scoped);
            container.Register<IBusinessProcessRepository, BusinessProcessRepository>(Lifestyle.Scoped);
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Scoped);
            container.Register<CorrelationIdMiddleware>(Lifestyle.Scoped);
            container.Register<EntryPointTelemetryScopeMiddleware>(Lifestyle.Scoped);
            container.Register<ServiceBusActorContextMiddleware>(Lifestyle.Scoped);
            container.Register<IActorContext, ActorContext>(Lifestyle.Scoped);
            container.Register<IDomainEventPublisher, DomainEventPublisher>();
            container.Register<IUnitOfWork, UnitOfWork>();
            container.Register<IValidator<CreateMeteringPoint>, RuleSet>(Lifestyle.Scoped);
            container.Register<IValidator<MasterDataDocument>, ValidationRuleSet>(Lifestyle.Scoped);
            container.Register<IValidator<ConnectMeteringPointRequest>, ConnectMeteringPointRuleSet>(Lifestyle.Scoped);
            container.Register<IValidator<UpdateMasterDataRequest>, NullValidationSet<UpdateMasterDataRequest>>(Lifestyle.Scoped);
            container.Register<RequestCloseDownValidator>(Lifestyle.Scoped);
            container.Register<IValidator<DisconnectReconnectMeteringPointRequest>, DisconnectReconnectMeteringPointRuleSet>(Lifestyle.Scoped);
            container.Register(typeof(IBusinessProcessResultHandler<UpdateMasterDataRequest>), typeof(ChangeMasterDataResultHandler), Lifestyle.Scoped);
            container.Register(typeof(IBusinessProcessResultHandler<CreateMeteringPoint>), typeof(CreateMeteringPointResultHandler<CreateMeteringPoint>), Lifestyle.Scoped);
            container.Register(typeof(IBusinessProcessResultHandler<ConnectMeteringPointRequest>), typeof(ConnectMeteringPointResultHandler), Lifestyle.Scoped);
            container.Register(typeof(IBusinessProcessResultHandler<DisconnectReconnectMeteringPointRequest>), typeof(DisconnectReconnectMeteringPointResultHandler), Lifestyle.Scoped);
            container.Register<IOutbox, OutboxProvider>(Lifestyle.Scoped);
            container.Register<IOutboxMessageFactory, OutboxMessageFactory>(Lifestyle.Scoped);
            container.Register<IJsonSerializer, JsonSerializer>(Lifestyle.Singleton);
            container.Register<ISystemDateTimeProvider, SystemDateTimeProvider>(Lifestyle.Singleton);
            container.Register<IDomainEventsAccessor, DomainEventsAccessor>();
            container.Register<IDomainEventsDispatcher, DomainEventsDispatcher>();
            container.Register<IIncomingMessageRegistry, IncomingMessageRegistry>(Lifestyle.Transient);
            container.Register<ServiceBusMessageIdempotencyMiddleware>(Lifestyle.Scoped);
            container.Register<IProtobufMessageFactory, ProtobufMessageFactory>(Lifestyle.Singleton);
            container.Register<ICommandScheduler, CommandScheduler>(Lifestyle.Scoped);
            container.Register<INotificationReceiver, NotificationReceiver>(Lifestyle.Scoped);
            container.Register<IActorMessageService, ActorMessageService>(Lifestyle.Scoped);
            container.Register<MeteringPointPipelineContext>(Lifestyle.Scoped);
            container.Register<IActorProvider, ActorProvider>(Lifestyle.Scoped);
            container.Register<IBusinessProcessValidationContext, BusinessProcessValidationContext>(Lifestyle.Scoped);
            container.Register<IBusinessProcessCommandFactory, BusinessProcessCommandFactory>(Lifestyle.Scoped);
            container.Register(typeof(ProtobufOutboundMapper<>), typeof(ProtobufOutboundMapper<>).Assembly);
            container.Register(typeof(ProtobufInboundMapper<>), typeof(ProtobufInboundMapper<>).Assembly);
            container.Register<ProtobufOutboundMapperFactory>();
            container.Register<ProtobufInboundMapperFactory>();
            container.Register<HealthCheckEndpoint>(Lifestyle.Scoped);

            var serviceBusConnectionString =
                Environment.GetEnvironmentVariable("SHARED_SERVICE_BUS_SEND_CONNECTION_STRING");
            container.Register<ServiceBusSender>(
                () =>
                {
                    var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
                    return serviceBusClient.CreateSender("metering-point-master-data-response");
                },
                Lifestyle.Singleton);

            // TODO: remove this when infrastructure and application has been split into more assemblies.
            container.Register<IDocumentSerializer<ConfirmMessage>, ConfirmMessageXmlSerializer>(Lifestyle.Singleton);
            container.Register<IDocumentSerializer<RejectMessage>, RejectMessageXmlSerializer>(Lifestyle.Singleton);
            container.Register<IDocumentSerializer<GenericNotificationMessage>, GenericNotificationMessageXmlSerializer>(Lifestyle.Singleton);
            container.Register<IDocumentSerializer<AccountingPointCharacteristicsMessage>, AccountingPointCharacteristicsMessageXmlSerializer>(Lifestyle.Singleton);

            container.Register<IMessageHubDispatcher, MessageHubDispatcher>(Lifestyle.Scoped);

            // TODO: NumberOfDaysEffectiveDateIsAllowedToBeforeToday uses days plus one.
            // So that if changes can be made 720 days back in time, NumberOfDaysEffectiveDateIsAllowedToBeforeToday needs to be 721.
            // This should probably be changed in the future.
            container.Register(() => new PolicyThresholds(NumberOfDaysEffectiveDateIsAllowedToBeforeToday: 721));
            container.Register(() => new ConnectSettings(
                NumberOfDaysEffectiveDateIsAllowedToBeforeToday: 721,
                NumberOfDaysEffectiveDateIsAllowedToAfterToday: 0));
            container.Register(() => new DisconnectReconnectSettings(
                NumberOfDaysEffectiveDateIsAllowedToBeforeToday: 721,
                NumberOfDaysEffectiveDateIsAllowedToAfterToday: 0));

            container.Register<IMeteringPointOwnershipProvider, MeteringPointOwnershipProvider>();
            container.Register<ParentCouplingService>(Lifestyle.Scoped);

            container.AddBusinessProcessAuthorizers();
            container.AddMasterDataValidators(typeof(IMasterDataValidatorStrategy).Assembly);
            container.AddMasterDataUpdateServices();

            container.AddBusinessRequestReceivers();

            container.Register(typeof(ProcessExtractor<>), typeof(CreateMeteringPointProcessExtractor), Lifestyle.Scoped);
            container.Register(typeof(ProcessExtractor<>), typeof(ConnectMeteringPointProcessExtractor), Lifestyle.Scoped);
            container.Register(typeof(ProcessExtractor<>), typeof(UpdateMeteringPointProcessExtractor), Lifestyle.Scoped);
            container.Register(typeof(ProcessExtractor<>), typeof(DisconnectReconnectMeteringPointProcessExtractor), Lifestyle.Scoped);
            container.RegisterConditional(typeof(ProcessExtractor<>), typeof(NullProcessExtractor<>), context => !context.Handled);

            container.UseMediatR()
                .WithPipeline(
                    typeof(UnitOfWorkBehavior<,>),
                    typeof(ProcessOverviewBehavior<,>),
                    typeof(InputValidationBehavior<,>),
                    typeof(DomainEventsDispatcherBehaviour<,>),
                    typeof(InternalCommandHandlingBehaviour<,>),
                    typeof(BusinessProcessResultBehavior<,>))
                .WithRequestHandlers(
                    typeof(UpdateMasterDataHandler),
                    typeof(MasterDataDocumentHandler),
                    typeof(DisconnectReconnectMeteringPointHandler),
                    typeof(CreateMeteringPointHandler),
                    typeof(AddEnergySupplierHandler),
                    typeof(ConnectMeteringPointHandler),
                    typeof(SendAccountingPointCharacteristicsMessageHandler),
                    typeof(SetEnergySupplierDetailsHandler),
                    typeof(CreateDefaultChargeLinksHandler),
                    typeof(MeteringPointByIdQueryHandler),
                    typeof(MeteringPointGsrnExistsQueryHandler),
                    typeof(EnergySuppliersByMeteringPointIdQueryHandler),
                    typeof(CloseDownMeteringPointHandler),
                    typeof(GetMasterDataQueryHandler))
                .WithNotificationHandlers(
                    typeof(CreateDefaultChargeLinksNotificationHandler),
                    typeof(MeteringPointCreatedNotificationHandler),
                    typeof(OnMeteringPointConnected),
                    typeof(OnMeteringPointDisconnected),
                    typeof(OnMeteringPointReconnected),
                    typeof(OnMasterDataWasUpdated),
                    typeof(SetEnergySupplierHACK),
                    typeof(ProcessInternalCommandsOnTimeHasPassed));

            Dapper.SqlMapper.AddTypeHandler(NodaTimeSqlMapper.Instance);

            container.ReceiveProtobuf<MeteringPointEnvelope>(
                config => config
                    .FromOneOf(envelope => envelope.MeteringPointMessagesCase)
                    .WithParser(() => MeteringPointEnvelope.Parser));

            container.SendProtobuf<MeteringPointEnvelope>();
        }
    }
}
