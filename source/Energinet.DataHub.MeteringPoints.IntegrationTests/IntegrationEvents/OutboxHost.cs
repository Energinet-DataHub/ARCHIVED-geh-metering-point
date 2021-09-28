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
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Application.Common.DomainEvents;
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Application.Validation;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.Common;
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
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Consumption;
using Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.PostOffice;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using FluentValidation;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Squadron;
using Xunit;
using Xunit.Categories;
using ConnectMeteringPoint = Energinet.DataHub.MeteringPoints.Contracts.ConnectMeteringPoint;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.IntegrationEvents
{
        [Collection("IntegrationTest")]
        public abstract class OutboxHost<TAzureCloudServiceBusOptions> : IDisposable, IClassFixture<
        AzureCloudServiceBusResource<TAzureCloudServiceBusOptions>>
            where TAzureCloudServiceBusOptions : AzureCloudServiceBusOptions, new()
        {
            private readonly AzureCloudServiceBusResource<TAzureCloudServiceBusOptions> _serviceBusResource;
            private readonly Scope _scope;
            private readonly Container _container;
            private readonly IServiceProvider _serviceProvider;
            private bool _disposed;

            protected OutboxHost(AzureCloudServiceBusResource<TAzureCloudServiceBusOptions> serviceBusResource)
            {
                _serviceBusResource = serviceBusResource;
                _container = new Container();
                _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

                // var databaseFixture = new DatabaseFixture();
                // var connectionString = databaseFixture.GetConnectionString();
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddDbContext<MeteringPointContext>(
                    x =>
                        x.UseSqlServer("Server=localhost;Database=MeteringPointTestDB;Trusted_Connection=True;", y => y.UseNodaTime()),
                    ServiceLifetime.Scoped);

                serviceCollection.AddSimpleInjector(_container);
                _serviceProvider = serviceCollection.BuildServiceProvider().UseSimpleInjector(_container);
                _container.Register(
                    () => new PostOfficeStorageClientSettings(
                        "something",
                        "temppostoffice"));
                _container.Register<IOutboxMessageDispatcher, OutboxMessageDispatcher>(Lifestyle.Scoped);
                _container.Register<IUnitOfWork, UnitOfWork>(Lifestyle.Scoped);
                _container.Register<ISystemDateTimeProvider, SystemDateTimeProviderStub>(Lifestyle.Scoped);
                _container.Register<IOutboxManager, OutboxManager>(Lifestyle.Scoped);
                _container.Register<OutboxOrchestrator>(Lifestyle.Scoped);
                _container.Register<IPostOfficeStorageClient, TempPostOfficeStorageClient>(Lifestyle.Scoped);
                _container.Register<IIntegrationMetaDataContext, IntegrationMetaDataContext>(Lifestyle.Scoped);
                _container
                    .Register<IJsonSerializer,
                        Energinet.DataHub.MeteringPoints.Infrastructure.Serialization.JsonSerializer>(Lifestyle.Scoped);

                _container.Register<ServiceBusClient>(
                    () => new ServiceBusClient(serviceBusResource.ConnectionString),
                    Lifestyle.Scoped);
                _container.Register(
                    () => new MeteringPointCreatedTopic("metering-point-created"),
                    Lifestyle.Scoped);
                _container.Register(
                    () => new MeteringPointConnectedTopic("metering-point-connected"),
                    Lifestyle.Scoped);
                _container.Register(
                    () => new ConsumptionMeteringPointCreatedTopic("consumption-metering-point-created"),
                    Lifestyle.Scoped);
                _container.Register(typeof(ITopicSender<>), typeof(TopicSender<>), Lifestyle.Scoped);

                _container.SendProtobuf<IntegrationEventEnvelope>();
                _container.BuildMediator(
                    new[] { typeof(OutboxWatcher).Assembly },
                    Array.Empty<Type>());

                _container.Verify();

                _scope = AsyncScopedLifestyle.BeginScope(_container);

                CleanupDatabase();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (_disposed == true)
                {
                    return;
                }

                CleanupDatabase();
                _scope.Dispose();
                ((ServiceProvider)_serviceProvider).Dispose();
                _container.Dispose();
                _disposed = true;
            }

            protected TService GetService<TService>()
                where TService : class
            {
                return _container.GetInstance<TService>();
            }

            protected ISubscriptionClient GetSubscription(string topicName, string subscription)
            {
                return _serviceBusResource.GetSubscriptionClient(topicName, subscription);
            }

            private void CleanupDatabase()
            {
                var cleanupStatement = new StringBuilder();

                cleanupStatement.AppendLine($"DELETE FROM OutboxMessages");

                _container.GetInstance<MeteringPointContext>()
                    .Database.ExecuteSqlRaw(cleanupStatement.ToString());
            }
        }
    }
