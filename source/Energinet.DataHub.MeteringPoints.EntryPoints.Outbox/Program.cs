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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.ContainerExtensions;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using GreenEnergyHub.PostOffice.Communicator.DataAvailable;
using GreenEnergyHub.PostOffice.Communicator.Dequeue;
using GreenEnergyHub.PostOffice.Communicator.Factories;
using GreenEnergyHub.PostOffice.Communicator.Model;
using GreenEnergyHub.PostOffice.Communicator.Peek;
using GreenEnergyHub.PostOffice.Communicator.Storage;
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
            container.Register<OutboxMessageFactory>(Lifestyle.Scoped);
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Scoped);

            var connectionString = Environment.GetEnvironmentVariable("SHARED_INTEGRATION_EVENT_SERVICE_BUS_SENDER_CONNECTION_STRING");
            container.Register(() => new ServiceBusClient(connectionString), Lifestyle.Singleton);

            container.Register(
                () => new MeteringPointCreatedTopic(Environment.GetEnvironmentVariable("METERING_POINT_CREATED_TOPIC") ?? throw new InvalidOperationException(
                    "No MeteringPointCreated Topic found")),
                Lifestyle.Singleton);
            container.Register(
                () => new MeteringPointConnectedTopic(Environment.GetEnvironmentVariable("METERING_POINT_CONNECTED_TOPIC") ?? throw new InvalidOperationException(
                    "No MeteringPointConnected Topic found")),
                Lifestyle.Singleton);

            container.Register(typeof(ITopicSender<>), typeof(TopicSender<>), Lifestyle.Singleton);

            container.SendProtobuf<IntegrationEventEnvelope>();

            container.AddSubPostOfficeDataAvailableClient();
            ConfigurePostOfficeDependencies(container); // TODO: temporary until AddPostOfficeCommunication extension method works as expected.

            // Setup pipeline behaviors
            container.BuildMediator(
                new[]
                {
                    typeof(OutboxWatcher).Assembly,
                },
                Array.Empty<Type>());
        }

        private static void ConfigurePostOfficeDependencies(Container container)
        {
            container.RegisterSingleton<IServiceBusClientFactory>(() => new ServiceBusClientFactory("connectionString"));
            container.Register<IDataAvailableNotificationSender, DataAvailableNotificationSender>(Lifestyle.Singleton);
            container.Register<IRequestBundleParser, RequestBundleParser>(Lifestyle.Singleton);
            container.Register<IResponseBundleParser, ResponseBundleParser>(Lifestyle.Singleton);
            container.Register<IDataBundleResponseSender>(() => new DataBundleResponseSender(container.GetRequiredService<IResponseBundleParser>(), container.GetRequiredService<IServiceBusClientFactory>(), DomainOrigin.MeteringPoints), Lifestyle.Singleton);
            container.Register<IStorageHandler, StorageHandler>(Lifestyle.Singleton);
            container.Register<IStorageServiceClientFactory>(() => new StorageServiceClientFactory("connectionString"), Lifestyle.Singleton);
            container.Register<IDequeueNotificationParser, DequeueNotificationParser>(Lifestyle.Singleton);
        }
    }
}
