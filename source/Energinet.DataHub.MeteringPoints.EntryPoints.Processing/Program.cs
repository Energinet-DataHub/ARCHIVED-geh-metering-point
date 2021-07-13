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
using Energinet.DataHub.MeteringPoints.Application.Common.DomainEvents;
using Energinet.DataHub.MeteringPoints.Application.Common.Users;
using Energinet.DataHub.MeteringPoints.Application.Validation;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.SimpleInjector;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Pipeline;
using Energinet.DataHub.MeteringPoints.Infrastructure.ContainerExtensions;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.DomainEventDispatching;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ConnectMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Energinet.DataHub.MeteringPoints.Infrastructure.Helpers;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Handlers;
using Energinet.DataHub.MeteringPoints.Infrastructure.Messaging.Idempotency;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.Infrastructure.UserIdentity;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SimpleInjector;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
{
    public static class Program
    {
        public static async Task Main()
        {
            var container = new Container();
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(options =>
                {
                    options.UseMiddleware<SimpleInjectorScopedRequest>();
                    options.UseMiddleware<ServiceBusCorrelationIdMiddleware>();
                    options.UseMiddleware<ServiceBusUserContextMiddleware>();
                    options.UseMiddleware<ServiceBusMessageIdempotencyMiddleware>();
                })
                .ConfigureServices(services =>
                {
                    var descriptor = new ServiceDescriptor(
                        typeof(IFunctionActivator),
                        typeof(SimpleInjectorActivator),
                        ServiceLifetime.Singleton);
                    services.Replace(descriptor); // Replace existing activator

                    services.AddLogging();
                    services.AddDbContext<MeteringPointContext>(x =>
                    {
                        var connectionString = Environment.GetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING")
                                               ?? throw new InvalidOperationException(
                                                   "Metering point db connection string not found.");

                        x.UseSqlServer(connectionString, y => y.UseNodaTime());
                    });
                    services.AddSimpleInjector(container, options =>
                    {
                        options.AddLogging();
                    });

                    services.ReceiveProtobuf<MeteringPointEnvelope>(
                        config => config
                            .FromOneOf(envelope => envelope.MeteringPointMessagesCase)
                            .WithParser(() => MeteringPointEnvelope.Parser));
                })
                .Build()
                .UseSimpleInjector(container);

            // Register application components.
            container.Register<QueueSubscriber>(Lifestyle.Scoped);
            container.Register<IntegrationEventReceiver>(Lifestyle.Scoped);
            container.Register<IMeteringPointRepository, MeteringPointRepository>(Lifestyle.Scoped);
            container.Register<ServiceBusCorrelationIdMiddleware>(Lifestyle.Scoped);
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Scoped);
            container.Register<ServiceBusUserContextMiddleware>(Lifestyle.Scoped);
            container.Register<IUserContext, UserContext>(Lifestyle.Scoped);
            container.Register<UserIdentityFactory>(Lifestyle.Singleton);
            container.Register<IDomainEventPublisher, DomainEventPublisher>();
            container.Register<IUnitOfWork, UnitOfWork>();
            container.Register<IValidator<Application.CreateMeteringPoint>, CreateMeteringPointRuleSet>(Lifestyle.Scoped);
            container.Register<IValidator<Application.ConnectMeteringPoint>, ConnectMeteringPointRuleSet>(Lifestyle.Scoped);
            container.Register(typeof(IBusinessProcessResultHandler<Application.CreateMeteringPoint>), typeof(CreateMeteringPointResultHandler), Lifestyle.Scoped);
            container.Register(typeof(IBusinessProcessResultHandler<Application.ConnectMeteringPoint>), typeof(ConnectMeteringPointResultHandler), Lifestyle.Scoped);
            container.Register<IOutbox, OutboxProvider>(Lifestyle.Scoped);
            container.Register<IOutboxMessageFactory, OutboxMessageFactory>(Lifestyle.Scoped);
            container.Register<IJsonSerializer, JsonSerializer>(Lifestyle.Singleton);
            container.Register<ISystemDateTimeProvider, SystemDateTimeProvider>(Lifestyle.Singleton);
            container.Register<IDomainEventsAccessor, DomainEventsAccessor>();
            container.Register<IDomainEventsDispatcher, DomainEventsDispatcher>();
            container.Register<IIncomingMessageRegistry, IncomingMessageRegistry>(Lifestyle.Transient);
            container.Register<ServiceBusMessageIdempotencyMiddleware>(Lifestyle.Scoped);
            container.Register<IProtobufMessageFactory, ProtobufMessageFactory>(Lifestyle.Singleton);

            container.AddValidationErrorConversion(
                validateRegistrations: true,
                typeof(Application.CreateMeteringPoint).Assembly, // Application
                typeof(MeteringPoint).Assembly, // Domain
                typeof(ErrorMessageFactory).Assembly); // Infrastructure

            // Setup pipeline behaviors
            container.BuildMediator(
                new[]
                {
                    typeof(Application.CreateMeteringPoint).Assembly,
                    typeof(MeteringPointCreatedNotificationHandler).Assembly,
                },
                new[]
                {
                    typeof(UnitOfWorkBehavior<,>),
                    typeof(InputValidationBehavior<,>),
                    typeof(AuthorizationBehavior<,>),
                    typeof(BusinessProcessResultBehavior<,>),
                    // TODO: NotImplementedException -> typeof(ValidationReportsBehavior<,>),
                    typeof(DomainEventsDispatcherBehaviour<,>),
                });

            container.Verify();

            await host.RunAsync().ConfigureAwait(false);

            await container.DisposeAsync().ConfigureAwait(false);
        }
    }
}
