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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Application.Common.DomainEvents;
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Application.GridAreas;
using Energinet.DataHub.MeteringPoints.Application.Validation;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Pipeline;
using Energinet.DataHub.MeteringPoints.Infrastructure.ContainerExtensions;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.GridAreas;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.DomainEventDispatching;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ConnectMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Contracts;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;
using Xunit.Categories;
using ConnectMeteringPoint = Energinet.DataHub.MeteringPoints.Application.Connect.ConnectMeteringPoint;
using CreateMeteringPoint = Energinet.DataHub.MeteringPoints.Application.Create.CreateMeteringPoint;
using JsonSerializer = Energinet.DataHub.MeteringPoints.Infrastructure.Serialization.JsonSerializer;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    [Collection("IntegrationTest")]
    [IntegrationTest]
    #pragma warning disable CA1724 // TODO: TestHost is reserved. Maybe refactor to base EntryPoint?
    public class TestHost : IDisposable
    {
        private readonly Scope _scope;
        private readonly Container _container;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed;

        protected TestHost(DatabaseFixture databaseFixture)
        {
            if (databaseFixture == null) throw new ArgumentNullException(nameof(databaseFixture));

            _container = new Container();
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var connectionString = databaseFixture.GetConnectionString();

            var serviceCollection = new ServiceCollection();

            // Protobuf handling
            _container.ReceiveProtobuf<MeteringPointEnvelope>(
                config => config
                    .FromOneOf(envelope => envelope.MeteringPointMessagesCase)
                    .WithParser(() => MeteringPointEnvelope.Parser));
            _container.SendProtobuf<MeteringPointEnvelope>();

            serviceCollection.AddDbContext<MeteringPointContext>(
                x =>
                x.UseSqlServer(connectionString, y => y.UseNodaTime()),
                ServiceLifetime.Scoped);
            serviceCollection.AddSimpleInjector(_container);
            _serviceProvider = serviceCollection.BuildServiceProvider().UseSimpleInjector(_container);

            _container.Register<IUnitOfWork, UnitOfWork>(Lifestyle.Scoped);
            _container.Register<IMeteringPointRepository, MeteringPointRepository>(Lifestyle.Scoped);
            _container.Register<IGridAreaRepository, GridAreaRepository>(Lifestyle.Scoped);
            _container.Register<IMarketMeteringPointRepository, MarketMeteringPointRepository>(Lifestyle.Scoped);
            _container.Register<IOutbox, OutboxProvider>(Lifestyle.Scoped);
            _container.Register<IOutboxManager, OutboxManager>(Lifestyle.Scoped);
            _container.Register<IOutboxMessageFactory, OutboxMessageFactory>(Lifestyle.Singleton);
            _container.Register<IJsonSerializer, JsonSerializer>(Lifestyle.Singleton);
            _container.Register<ISystemDateTimeProvider, SystemDateTimeProviderStub>(Lifestyle.Singleton);
            _container.Register(typeof(IBusinessProcessResultHandler<CreateMeteringPoint>), typeof(CreateMeteringPointResultHandler), Lifestyle.Scoped);
            _container.Register(typeof(IBusinessProcessResultHandler<ConnectMeteringPoint>), typeof(ConnectMeteringPointResultHandler), Lifestyle.Scoped);
            _container.Register<IValidator<CreateMeteringPoint>, CreateMeteringPointRuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<ConnectMeteringPoint>, ConnectMeteringPointRuleSet>(Lifestyle.Scoped);
            _container.Register<IDomainEventsAccessor, DomainEventsAccessor>();
            _container.Register<IDomainEventsDispatcher, DomainEventsDispatcher>();
            _container.Register<IDomainEventPublisher, DomainEventPublisher>();
            _container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Singleton);
            _container.Register<ICommandScheduler, CommandScheduler>(Lifestyle.Scoped);

            _container.Register<IDbConnectionFactory>(() => new SqlDbConnectionFactory(connectionString), Lifestyle.Scoped);

            _container.AddValidationErrorConversion(
                validateRegistrations: true,
                typeof(CreateMeteringPoint).Assembly, // Application
                typeof(MeteringPoint).Assembly, // Domain
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
                    // typeof(AuthorizationBehavior<,>),
                    typeof(InputValidationBehavior<,>),
                    typeof(DomainEventsDispatcherBehaviour<,>),
                    typeof(InternalCommandHandlingBehaviour<,>),
                    typeof(BusinessProcessResultBehavior<,>),
                });

            _container.Verify();

            _scope = AsyncScopedLifestyle.BeginScope(_container);

            _container.GetInstance<ICorrelationContext>().SetId(Guid.NewGuid().ToString().Replace("-", string.Empty, StringComparison.Ordinal));

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

        protected IEnumerable<TMessage> GetOutboxMessages<TMessage>()
        {
            var jsonSerializer = GetService<IJsonSerializer>();
            var context = GetService<MeteringPointContext>();
            return context.OutboxMessages
                .Where(message => message.Type == typeof(TMessage).FullName)
                .Select(message => jsonSerializer.Deserialize<TMessage>(message.Data));
        }

        protected void AssertOutboxMessage<TMessage>(Func<TMessage, bool> funcAssert)
        {
            if (funcAssert == null) throw new ArgumentNullException(nameof(funcAssert));

            var message = GetOutboxMessages<TMessage>().SingleOrDefault(funcAssert.Invoke);

            message.Should().NotBeNull();
            message.Should().BeOfType<TMessage>();
        }

        protected void AssertOutboxMessage<TMessage>()
        {
            var message = GetOutboxMessages<TMessage>().SingleOrDefault();

            message.Should().NotBeNull();
            message.Should().BeOfType<TMessage>();
        }

        protected void AssertValidationError<TRejectMessage>(string expectedErrorCode)
            where TRejectMessage : IRejectMessage
        {
            var message = GetOutboxMessages
                    <PostOfficeEnvelope>()
                .First(msg => msg.MessageType.Equals(typeof(TRejectMessage).Name, StringComparison.Ordinal));

            var rejectMessage = JsonConvert.DeserializeObject<TRejectMessage>(message.Content);
            var validationError = rejectMessage.Errors
                .First(error => error.Code == expectedErrorCode);

            Assert.NotNull(validationError);
        }

        protected async Task SendCommandAsync(object command, CancellationToken cancellationToken = default)
        {
            await using var scope = AsyncScopedLifestyle.BeginScope(_container);
            await scope.GetInstance<IMediator>().Send(command, cancellationToken).ConfigureAwait(false);
        }

        private void CleanupDatabase()
        {
            var cleanupStatement = new StringBuilder();

            cleanupStatement.AppendLine($"DELETE FROM ConsumptionMeteringPoints");
            cleanupStatement.AppendLine($"DELETE FROM ProductionMeteringPoints");
            cleanupStatement.AppendLine($"DELETE FROM ExchangeMeteringPoints");
            cleanupStatement.AppendLine($"DELETE FROM MarketMeteringPoints");
            cleanupStatement.AppendLine($"DELETE FROM MeteringPoints");
            cleanupStatement.AppendLine($"DELETE FROM OutboxMessages");
            cleanupStatement.AppendLine($"DELETE FROM QueuedInternalCommands");

            _container.GetInstance<MeteringPointContext>()
                .Database.ExecuteSqlRaw(cleanupStatement.ToString());
        }
    }
}
