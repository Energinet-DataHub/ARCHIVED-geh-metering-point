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
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.SimpleInjector;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Helpers;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Channels;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Dispatchers;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Handlers;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Helpers;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Repository;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Services;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SimpleInjector;

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
                        var connectionString = Environment.GetEnvironmentVariable("METERING_POINT_DB_CONNECTION_STRING")
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
            container.Register<IJsonSerializer, JsonSerializer>(Lifestyle.Singleton);
            container.Register<IIntegrationEventRepository, IntegrationEventRepository>();
            container.Register<EventMessageDispatcher>(Lifestyle.Transient);
            container.Register<IntegrationEventToEventHubDispatcher>(Lifestyle.Transient);
            container.Register<AzureEventHubChannel>(Lifestyle.Transient);
            container.Register<IIntegrationEventDispatchOrchestrator, IntegrationEventDispatchOrchestrator>(Lifestyle.Transient);
            container.Register<CreateMeteringPointEventHandler>(Lifestyle.Transient);
            container.Register<IUnitOfWork, UnitOfWork>(Lifestyle.Scoped);

            container.BuildMediator(
                new[] { typeof(CreateMeteringPointEventMessage).Assembly }, Array.Empty<Type>());

            container.Verify();

            await host.RunAsync().ConfigureAwait(false);

            await container.DisposeAsync().ConfigureAwait(false);
        }
    }
}
