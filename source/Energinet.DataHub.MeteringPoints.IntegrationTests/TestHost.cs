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
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Common.DomainEvents;
using Energinet.DataHub.MeteringPoints.Application.IntegrationEvent;
using Energinet.DataHub.MeteringPoints.Application.Validation;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Pipeline;
using Energinet.DataHub.MeteringPoints.Infrastructure.ContainerExtensions;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.DomainEventDispatching;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Energinet.DataHub.MeteringPoints.Infrastructure.Helpers;
using Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Channels;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Dispatchers;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Repository;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices.Services;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    public class TestHost : IDisposable
    {
        private readonly Scope _scope;
        private readonly Container _container;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed;

        protected TestHost()
        {
            CleanupDatabase();

            _container = new Container();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<MeteringPointContext>(x =>
                x.UseSqlServer(ConnectionString, y => y.UseNodaTime()));
            serviceCollection.AddSimpleInjector(_container);
            _serviceProvider = serviceCollection.BuildServiceProvider().UseSimpleInjector(_container);

            _container.Register<IUnitOfWork, UnitOfWork>(Lifestyle.Scoped);
            _container.Register<IMeteringPointRepository, MeteringPointRepository>(Lifestyle.Scoped);
            _container.Register<IOutbox, InMemoryOutbox>(Lifestyle.Scoped);
            _container.Register<IOutboxManager, InMemoryOutbox>(Lifestyle.Scoped);
            _container.Register<IOutboxMessageFactory, OutboxMessageFactory>(Lifestyle.Singleton);
            _container.Register<IJsonSerializer, JsonSerializer>(Lifestyle.Singleton);
            _container.Register<ISystemDateTimeProvider, SystemDateTimeProviderStub>(Lifestyle.Singleton);
            _container.Register(typeof(IBusinessProcessResultHandler<>), typeof(CreateMeteringPointResultHandler), Lifestyle.Scoped);
            _container.Register<IValidator<CreateMeteringPoint>, CreateMeteringPointRuleSet>(Lifestyle.Scoped);
            _container.Register<IDomainEventsAccessor, DomainEventsAccessor>();
            _container.Register<IDomainEventsDispatcher, DomainEventsDispatcher>();
            _container.Register<IDomainEventPublisher, DomainEventPublisher>();
            _container.AddValidationErrorConversion(
                validateRegistrations: true,
                typeof(CreateMeteringPoint).Assembly, // Application
                typeof(GsrnNumberMustBeValidValidationError).Assembly, // Domain
                typeof(ErrorMessageFactory).Assembly); // Infrastructure

            _container.BuildMediator(
                new[]
                {
                    typeof(CreateMeteringPoint).Assembly,
                    typeof(MeteringPointCreatedNotificationHandler).Assembly,
                },
                new[]
                {
                    typeof(UnitOfWorkBehavior<,>),
                    typeof(InputValidationBehavior<,>),
                    // typeof(AuthorizationBehavior<,>),
                    typeof(BusinessProcessResultBehavior<,>),
                    typeof(DomainEventsDispatcherBehaviour<,>),
                    // typeof(ValidationReportsBehavior<,>),
                });

            // _container.Verify();
            _scope = AsyncScopedLifestyle.BeginScope(_container);
        }

        private string ConnectionString =>
            Environment.GetEnvironmentVariable("MeteringPoints_IntegrationTests_ConnectionString");

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

        private void CleanupDatabase()
        {
            // new SqlCommand(cleanupStatement, GetSqlDbConnection()).ExecuteNonQuery();
            var cleanupStatement = $"";
        }
    }
}
