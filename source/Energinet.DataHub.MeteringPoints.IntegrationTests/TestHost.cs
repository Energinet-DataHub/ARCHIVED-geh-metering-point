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
using Energinet.DataHub.MeteringPoints.Application.ChangeMasterData;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Application.Common.DomainEvents;
using Energinet.DataHub.MeteringPoints.Application.Common.Users;
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Application.Create.Consumption;
using Energinet.DataHub.MeteringPoints.Application.GridAreas;
using Energinet.DataHub.MeteringPoints.Application.GridAreas.Create;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Application.Validation;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
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
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ChangeMasterData;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ConnectMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.GridAreas;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands;
using Energinet.DataHub.MeteringPoints.Infrastructure.LocalMessageHub.Bundling;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.Infrastructure.UserIdentity;
using Energinet.DataHub.MeteringPoints.IntegrationTests.MarketDocuments;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Validators;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;
using Xunit.Categories;
using Xunit.Sdk;
using ConnectMeteringPoint = Energinet.DataHub.MeteringPoints.Application.Connect.ConnectMeteringPoint;
using JsonSerializer = Energinet.DataHub.MeteringPoints.Infrastructure.Serialization.JsonSerializer;
using MasterDataDocument = Energinet.DataHub.MeteringPoints.Application.MarketDocuments.MasterDataDocument;

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
        private JsonSerializer _jsonSerializer = new();

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
            _container.Register(typeof(IBusinessProcessResultHandler<ChangeMasterDataRequest>), typeof(ChangeMasterDataResultHandler), Lifestyle.Scoped);
            _container.Register(typeof(IBusinessProcessResultHandler<CreateConsumptionMeteringPoint>), typeof(CreateMeteringPointResultHandler), Lifestyle.Scoped);
            _container.Register(typeof(IBusinessProcessResultHandler<ConnectMeteringPoint>), typeof(ConnectMeteringPointResultHandler), Lifestyle.Scoped);
            _container.Register(typeof(IBusinessProcessResultHandler<CreateGridArea>), typeof(CreateGridAreaNullResultHandler), Lifestyle.Singleton);
            _container.Register<IValidator<MasterDataDocument>, ValidationRuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<ConnectMeteringPoint>, ConnectMeteringPointRuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<CreateGridArea>, CreateGridAreaRuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<CreateConsumptionMeteringPoint>, Application.Create.Consumption.Validation.RuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<ChangeMasterDataRequest>, NullValidationSet<ChangeMasterDataRequest>>(Lifestyle.Scoped);
            _container.Register<IDomainEventsAccessor, DomainEventsAccessor>();
            _container.Register<IDomainEventsDispatcher, DomainEventsDispatcher>();
            _container.Register<IDomainEventPublisher, DomainEventPublisher>();
            _container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Singleton);
            _container.Register<ICommandScheduler, CommandScheduler>(Lifestyle.Scoped);
            _container.Register<IUserContext>(() => new UserContext { CurrentUser = new UserIdentity(Guid.NewGuid().ToString(), "8200000001409"), }, Lifestyle.Scoped);

            _container.Register<IDbConnectionFactory>(() => new SqlDbConnectionFactory(connectionString), Lifestyle.Scoped);

            _container.Register<IBusinessProcessValidationContext, BusinessProcessValidationContext>(Lifestyle.Scoped);
            _container.Register<IBusinessProcessCommandFactory, BusinessProcessCommandFactory>(Lifestyle.Singleton);
            _container.Register(typeof(IBusinessProcessResultHandler<TestBusinessRequest>), typeof(TestBusinessRequestResultHandler), Lifestyle.Scoped);

            // TODO: remove this when infrastructure and application has been split into more assemblies.
            _container.Register<IDocumentSerializer<ConfirmMessage>, ConfirmMessageSerializer>(Lifestyle.Singleton);
            _container.Register<IDocumentSerializer<RejectMessage>, RejectMessageSerializer>(Lifestyle.Singleton);

            _container.Register<IActorMessageFactory, ActorMessageFactory>(Lifestyle.Scoped);
            _container.Register<IMessageHubDispatcher, MessageHubDispatcher>(Lifestyle.Scoped);

            _container.AddValidationErrorConversion(
                validateRegistrations: true,
                typeof(MasterDataDocument).Assembly, // Application
                typeof(MeteringPoint).Assembly, // Domain
                typeof(ErrorMessageFactory).Assembly); // Infrastructure

            _container.BuildMediator(
                new[]
                {
                    typeof(MasterDataDocument).Assembly,
                    typeof(MeteringPointCreatedNotificationHandler).Assembly,
                },
                new[]
                {
                    typeof(UnitOfWorkBehavior<,>),
                    // typeof(AuthorizationBehavior<,>),
                    typeof(InputValidationBehavior<,>), typeof(DomainEventsDispatcherBehaviour<,>), typeof(InternalCommandHandlingBehaviour<,>), typeof(BusinessProcessResultBehavior<,>),
                });

            _container.Verify();

            _scope = AsyncScopedLifestyle.BeginScope(_container);

            _container.GetInstance<ICorrelationContext>().SetId(Guid.NewGuid().ToString().Replace("-", string.Empty, StringComparison.Ordinal));
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

        protected void AssertValidationError(string expectedErrorCode, DocumentType type)
        {
            var message = GetOutboxMessages
                    <MessageHubEnvelope>()
                .Single(msg => msg.MessageType.Equals(type));

            var rejectMessage = _jsonSerializer.Deserialize<RejectMessage>(message.Content);

            var errorCount = rejectMessage.MarketActivityRecord.Reasons.Count;
            if (errorCount > 1)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"Reject message contains more ({errorCount}) than 1 error:");
                foreach (var error in rejectMessage.MarketActivityRecord.Reasons)
                {
                    errorMessage.AppendLine($"Code: {error.Code}. Description: {error.Text}.");
                }

                throw new XunitException(errorMessage.ToString());
            }

            var validationError = rejectMessage.MarketActivityRecord.Reasons
                .Single(error => error.Code == expectedErrorCode);

            Assert.NotNull(validationError);
        }

        protected void AssertValidationError(string expectedErrorCode)
        {
            var message = GetOutboxMessages
                    <MessageHubEnvelope>()
                .Single(msg => msg.MessageType.Name.EndsWith("rejected", StringComparison.OrdinalIgnoreCase));

            var rejectMessage = GetService<IJsonSerializer>().Deserialize<RejectMessage>(message.Content);

            var errorCount = rejectMessage.MarketActivityRecord.Reasons.Count;
            if (errorCount > 1)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"Reject message contains more ({errorCount}) than 1 error:");
                foreach (var error in rejectMessage.MarketActivityRecord.Reasons)
                {
                    errorMessage.AppendLine($"Code: {error.Code}. Description: {error.Text}.");
                }

                throw new XunitException(errorMessage.ToString());
            }

            var validationError = rejectMessage.MarketActivityRecord.Reasons
                .Single(error => error.Code == expectedErrorCode);

            Assert.NotNull(validationError);
        }

        protected TIntegrationEvent? FindIntegrationEvent<TIntegrationEvent>()
        {
            return GetOutboxMessages<TIntegrationEvent>().SingleOrDefault();
        }

        protected void AssertConfirmMessage(DocumentType documentType)
        {
            var message = GetOutboxMessages
                    <MessageHubEnvelope>()
                .Single(msg => msg.MessageType.Equals(documentType));

            var confirmMessage = GetService<IJsonSerializer>().Deserialize<ConfirmMessage>(message.Content);

            Assert.NotNull(confirmMessage);
        }

        protected void AseertNoIntegrationEventIsRaised<TIntegrationEvent>()
        {
            Assert.Null(GetOutboxMessages<TIntegrationEvent>().SingleOrDefault());
        }

        protected async Task<BusinessProcessResult> InvokeBusinessProcessAsync(IBusinessRequest request)
        {
            var result = await GetService<IMediator>().Send(request).ConfigureAwait(false);
            return result;
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
            cleanupStatement.AppendLine($"DELETE FROM GridAreaLinks");
            cleanupStatement.AppendLine($"DELETE FROM GridAreas");
            cleanupStatement.AppendLine($"DELETE FROM MessageHubMessages");

            _container.GetInstance<MeteringPointContext>()
                .Database.ExecuteSqlRaw(cleanupStatement.ToString());
        }
    }
}
