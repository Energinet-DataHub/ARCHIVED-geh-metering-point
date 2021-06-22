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
using Azure.Messaging.EventHubs.Producer;
using Energinet.DataHub.MeteringPoints.Application.IntegrationEvent;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.SimpleInjector;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.RequestHandlers;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Helpers;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Channels;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Dispatchers;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Services;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SimpleInjector;
using CreateMeteringPointEventMessage = Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Messages.CreateMeteringPointEventMessage;

[assembly: CLSCompliant(false)]

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Outbox
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

                    services.SendProtobuf<IntegrationEventEnvelope>();
                })
                .Build()
                .UseSimpleInjector(container);

            // Register application components.
            var eventHubConnectionString = Environment.GetEnvironmentVariable("METERINGPOINTEVENTHUB_CONNECTION_STRING");
            var hubName = Environment.GetEnvironmentVariable("METERINGPOINTEVENTHUB_HUB_NAME");
            container.Register<EventHubProducerClient>(
                () => new EventHubProducerClient(eventHubConnectionString, hubName),
                Lifestyle.Singleton);
            container.Register(
                () => new PostOfficeStorageClientSettings(
                    Environment.GetEnvironmentVariable("TEMP_POST_OFFICE_CONNECTION_STRING")!,
                    Environment.GetEnvironmentVariable("TEMP_POST_OFFICE_SHARE")!));
            container.Register<ISystemDateTimeProvider, SystemDateTimeProvider>(Lifestyle.Scoped);
            container.Register<IJsonSerializer, JsonSerializer>(Lifestyle.Singleton);
            container.Register<IOutbox, OutboxProvider>(Lifestyle.Scoped);
            container.Register<IOutboxManager, OutboxManager>(Lifestyle.Scoped);
            container.Register<IOutboxMessageFactory, OutboxMessageFactory>(Lifestyle.Scoped);
            container.Register<IUnitOfWork, UnitOfWork>(Lifestyle.Scoped);
            container.Register<EventMessageDispatcher>(Lifestyle.Transient);
            container.Register<IntegrationEventToEventHubDispatcher>(Lifestyle.Transient);
            container.Register<AzureEventHubChannel>(Lifestyle.Transient);
            container.Register<IIntegrationEventDispatchOrchestrator, IntegrationEventDispatchOrchestrator>(Lifestyle.Transient);
            container.Register<CreateMeteringPointEventHandler>(Lifestyle.Transient);
            container.Register<IPostOfficeStorageClient, TempPostOfficeStorageClient>(Lifestyle.Scoped);

            container.BuildMediator(
                new[]
                {
                    typeof(CreateMeteringPointEventMessage).Assembly,
                    typeof(CreateMeteringPointEventHandler).Assembly,
                },
                Array.Empty<Type>());

            container.Verify();

            await host.RunAsync().ConfigureAwait(false);

            await container.DisposeAsync().ConfigureAwait(false);
        }
    }
}
