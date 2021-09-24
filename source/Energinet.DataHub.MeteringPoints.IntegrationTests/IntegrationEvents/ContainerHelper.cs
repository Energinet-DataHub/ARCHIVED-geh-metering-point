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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Squadron;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.IntegrationEvents
{
    public static class ContainerHelper
    {
        public static void CleanupDatabase(Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var cleanupStatement = new StringBuilder();

            cleanupStatement.AppendLine($"DELETE FROM OutboxMessages");

            container.GetInstance<MeteringPointContext>()
                .Database.ExecuteSqlRaw(cleanupStatement.ToString());
        }

        public static void CreateContainer(Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var databaseFixture = new DatabaseFixture();
            var connectionString = databaseFixture.GetConnectionString();
            var serviceCollection = new ServiceCollection();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            serviceCollection.AddDbContext<MeteringPointContext>(
                x =>
                    x.UseSqlServer(connectionString, y => y.UseNodaTime()),
                ServiceLifetime.Scoped);

            serviceCollection.AddSimpleInjector(container);
            serviceCollection.BuildServiceProvider().UseSimpleInjector(container);
            container.Register(
                () => new PostOfficeStorageClientSettings(
                    "DefaultEndpointsProtocol=https;AccountName=stormeteringpointtmpu;AccountKey=KwFnZJh3Tv/am6o8SdPeA/GplYwitkCsFt6GajCpRY1zoRkdyCrpfASWegYDYRlI+saRBY4ecL4+27D4sTFoQA==;EndpointSuffix=core.windows.net",
                    "temppostoffice"));
            container.Register<IOutboxMessageDispatcher, OutboxMessageDispatcher>(Lifestyle.Scoped);
            container.Register<IUnitOfWork, UnitOfWork>(Lifestyle.Scoped);
            container.Register<ISystemDateTimeProvider, SystemDateTimeProviderStub>(Lifestyle.Scoped);
            container.Register<IOutboxManager, OutboxManager>(Lifestyle.Scoped);
            container.Register<OutboxOrchestrator>(Lifestyle.Scoped);
            container.Register<IPostOfficeStorageClient, TempPostOfficeStorageClient>(Lifestyle.Scoped);
            container.Register<IIntegrationMetaDataContext, IntegrationMetaDataContext>(Lifestyle.Scoped);
            container.Register<IJsonSerializer, Energinet.DataHub.MeteringPoints.Infrastructure.Serialization.JsonSerializer>(Lifestyle.Scoped);
            container.SendProtobuf<IntegrationEventEnvelope>();
            container.BuildMediator(
                new[]
                {
                    typeof(OutboxWatcher).Assembly,
                },
                Array.Empty<Type>());
        }

        public static void VerifyContainer(Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            container.Verify();
        }

        public static void RegisterServiceBusService<TAzureCloudServiceBusOptions>(Container container, AzureCloudServiceBusResource<TAzureCloudServiceBusOptions> serviceBusResource)
        where TAzureCloudServiceBusOptions : AzureCloudServiceBusOptions, new()
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (serviceBusResource == null) throw new ArgumentNullException(nameof(serviceBusResource));
            container.Register<ServiceBusClient>(
                () => new ServiceBusClient(serviceBusResource.ConnectionString),
                Lifestyle.Scoped);
            container.Register(
                () => new MeteringPointCreatedTopic("metering-point-created"),
                Lifestyle.Scoped);
            container.Register(
                () => new MeteringPointConnectedTopic("metering-point-connected"),
                Lifestyle.Scoped);
            container.Register(typeof(ITopicSender<>), typeof(TopicSender<>), Lifestyle.Scoped);
        }
    }
}
