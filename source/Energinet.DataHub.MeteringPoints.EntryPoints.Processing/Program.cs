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
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.ChangeMasterData;
using Energinet.DataHub.MeteringPoints.Application.ChangeMasterData.Consumption;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Application.Common.DomainEvents;
using Energinet.DataHub.MeteringPoints.Application.Common.Users;
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.Create.Validation;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Application.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Application.Providers.MeteringPointOwnership;
using Energinet.DataHub.MeteringPoints.Application.Validation;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.Processing.Functions;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Authorization;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Pipeline;
using Energinet.DataHub.MeteringPoints.Infrastructure.ContainerExtensions;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.GridAreas;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MessageHub.Bundling;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.DomainEventDispatching;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ChangeMasterData;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ConnectMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Parties;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Notifications;
using Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands;
using Energinet.DataHub.MeteringPoints.Infrastructure.Messaging.Idempotency;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Providers.MeteringPointOwnership;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.Infrastructure.UserIdentity;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Confirm;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Reject;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
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
            options.UseMiddleware<ServiceBusUserContextMiddleware>();
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
        }

        protected override void ConfigureContainer(Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            base.ConfigureContainer(container);

            // Register application components.
            container.Register<QueueSubscriber>(Lifestyle.Scoped);

            var connectionString = Environment.GetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING")
                                   ?? throw new InvalidOperationException(
                                       "Metering point db connection string not found.");
            container.Register<IDbConnectionFactory>(() => new SqlDbConnectionFactory(connectionString), Lifestyle.Scoped);
            container.Register<DbGridAreaHelper>(Lifestyle.Scoped);
            container.Register<IntegrationEventReceiver>(Lifestyle.Scoped);
            container.Register<ChargesResponseReceiver>(Lifestyle.Scoped);
            container.Register<IMeteringPointRepository, MeteringPointRepository>(Lifestyle.Scoped);
            container.Register<IEnergySupplierRepository, EnergySupplierRepository>(Lifestyle.Scoped);
            container.Register<IGridAreaRepository, GridAreaRepository>(Lifestyle.Scoped);
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Scoped);
            container.Register<CorrelationIdMiddleware>(Lifestyle.Scoped);
            container.Register<EntryPointTelemetryScopeMiddleware>(Lifestyle.Scoped);
            container.Register<ServiceBusUserContextMiddleware>(Lifestyle.Scoped);
            container.Register<IUserContext, UserContext>(Lifestyle.Scoped);
            container.Register<UserIdentityFactory>(Lifestyle.Singleton);
            container.Register<IDomainEventPublisher, DomainEventPublisher>();
            container.Register<IUnitOfWork, UnitOfWork>();
            container.Register<IValidator<CreateMeteringPoint>, RuleSet>(Lifestyle.Scoped);
            container.Register<IValidator<MasterDataDocument>, ValidationRuleSet>(Lifestyle.Scoped);
            container.Register<IValidator<ConnectMeteringPointRequest>, ConnectMeteringPointRuleSet>(Lifestyle.Scoped);
            container.Register<IValidator<ChangeMasterDataRequest>, NullValidationSet<ChangeMasterDataRequest>>(Lifestyle.Scoped);
            container.Register(typeof(IBusinessProcessResultHandler<ChangeMasterDataRequest>), typeof(ChangeMasterDataResultHandler), Lifestyle.Scoped);
            container.Register(typeof(IBusinessProcessResultHandler<CreateMeteringPoint>), typeof(CreateMeteringPointResultHandler<CreateMeteringPoint>), Lifestyle.Scoped);
            container.Register(typeof(IBusinessProcessResultHandler<ConnectMeteringPointRequest>), typeof(ConnectMeteringPointResultHandler), Lifestyle.Scoped);
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
            container.Register<IBusinessDocumentFactory, BusinessDocumentFactory>(Lifestyle.Scoped);
            container.Register<MeteringPointPipelineContext>(Lifestyle.Scoped);
            container.Register<ActorProvider>(Lifestyle.Scoped);

            container.Register<IBusinessProcessValidationContext, BusinessProcessValidationContext>(Lifestyle.Scoped);
            container.Register<IBusinessProcessCommandFactory, BusinessProcessCommandFactory>(Lifestyle.Singleton);

            // TODO: remove this when infrastructure and application has been split into more assemblies.
            container.Register<IDocumentSerializer<ConfirmMessage>, ConfirmMessageXmlSerializer>(Lifestyle.Singleton);
            container.Register<IDocumentSerializer<RejectMessage>, RejectMessageXmlSerializer>(Lifestyle.Singleton);

            container.Register<IActorMessageFactory, ActorMessageFactory>(Lifestyle.Scoped);
            container.Register<IMessageHubDispatcher, MessageHubDispatcher>(Lifestyle.Scoped);

            container.Register<ChangeMasterDataSettings>(() => new ChangeMasterDataSettings(NumberOfDaysEffectiveDateIsAllowedToBeforeToday: 1));
            container.Register<ConnectSettings>(() => new ConnectSettings(
                NumberOfDaysEffectiveDateIsAllowedToBeforeToday: 7,
                NumberOfDaysEffectiveDateIsAllowedToAfterToday: 0));

            container.Register<IMeteringPointOwnershipProvider, MeteringPointOwnershipProvider>();

            container.AddBusinessProcessAuthorizers();

            container.AddValidationErrorConversion(
                validateRegistrations: false,
                typeof(MasterDataDocument).Assembly, // Application
                typeof(MeteringPoint).Assembly, // Domain
                typeof(ErrorMessageFactory).Assembly); // Infrastructure

            // Setup pipeline behaviors
            container.BuildMediator(
                new[]
                {
                    typeof(MasterDataDocument).Assembly,
                    typeof(MeteringPointCreatedNotificationHandler).Assembly,
                },
                new[]
                {
                    typeof(UnitOfWorkBehavior<,>),
                    typeof(InputValidationBehavior<,>),
                    typeof(AuthorizationBehavior<,>),
                    typeof(DomainEventsDispatcherBehaviour<,>),
                    typeof(InternalCommandHandlingBehaviour<,>),
                    typeof(BusinessProcessResultBehavior<,>),
                });

            container.ReceiveProtobuf<MeteringPointEnvelope>(
                config => config
                    .FromOneOf(envelope => envelope.MeteringPointMessagesCase)
                    .WithParser(() => MeteringPointEnvelope.Parser));

            container.SendProtobuf<MeteringPointEnvelope>();
        }
    }
}
