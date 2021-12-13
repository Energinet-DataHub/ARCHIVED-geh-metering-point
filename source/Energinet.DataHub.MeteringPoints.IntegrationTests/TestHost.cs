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
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Authorization;
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
using Energinet.DataHub.MeteringPoints.Application.GridAreas;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Application.Providers.MeteringPointOwnership;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.WebApi.GridAreas.Create;
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
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Providers.MeteringPointOwnership;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.Infrastructure.UserIdentity;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        private readonly JsonSerializer _jsonSerializer = new();
        private readonly Scope _scope;
        private readonly Container _container;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed;

        protected TestHost(DatabaseFixture databaseFixture)
        {
            if (databaseFixture == null)
                throw new ArgumentNullException(nameof(databaseFixture));
            databaseFixture.DatabaseManager.UpgradeDatabase();

            _container = new Container();
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var serviceCollection = new ServiceCollection();

            // Protobuf handling
            _container.ReceiveProtobuf<MeteringPointEnvelope>(
                config => config
                    .FromOneOf(envelope => envelope.MeteringPointMessagesCase)
                    .WithParser(() => MeteringPointEnvelope.Parser));
            _container.SendProtobuf<MeteringPointEnvelope>();

            serviceCollection.AddDbContext<MeteringPointContext>(
                x =>
                    x.UseSqlServer(databaseFixture.DatabaseManager.ConnectionString, y => y.UseNodaTime()),
                ServiceLifetime.Scoped);
            serviceCollection.AddLogging();
            serviceCollection.AddSimpleInjector(_container, options =>
            {
                options.AddLogging(); // Allow use non-generic ILogger interface
            });
            _serviceProvider = serviceCollection.BuildServiceProvider().UseSimpleInjector(_container);

            _container.Register<IUnitOfWork, UnitOfWork>(Lifestyle.Scoped);
            _container.Register<IMeteringPointRepository, MeteringPointRepository>(Lifestyle.Scoped);
            _container.Register<IEnergySupplierRepository, EnergySupplierRepository>(Lifestyle.Scoped);
            _container.Register<IGridAreaRepository, GridAreaRepository>(Lifestyle.Scoped);
            _container.Register<IOutbox, OutboxProvider>(Lifestyle.Scoped);
            _container.Register<IOutboxManager, OutboxManager>(Lifestyle.Scoped);
            _container.Register<IOutboxMessageFactory, OutboxMessageFactory>(Lifestyle.Singleton);
            _container.Register<IJsonSerializer, JsonSerializer>(Lifestyle.Singleton);
            _container.Register<ISystemDateTimeProvider, SystemDateTimeProviderStub>(Lifestyle.Singleton);
            _container.Register(typeof(IBusinessProcessResultHandler<ChangeMasterDataRequest>), typeof(ChangeMasterDataResultHandler), Lifestyle.Scoped);
            _container.Register(typeof(IBusinessProcessResultHandler<CreateMeteringPoint>), typeof(CreateMeteringPointResultHandler<CreateMeteringPoint>), Lifestyle.Scoped);
            _container.Register(typeof(IBusinessProcessResultHandler<ConnectMeteringPoint>), typeof(ConnectMeteringPointResultHandler), Lifestyle.Scoped);
            _container.Register(typeof(IBusinessProcessResultHandler<CreateGridArea>), typeof(CreateGridAreaNullResultHandler), Lifestyle.Singleton);
            _container.Register<IValidator<MasterDataDocument>, ValidationRuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<ConnectMeteringPoint>, ConnectMeteringPointRuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<CreateGridArea>, CreateGridAreaRuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<CreateMeteringPoint>, RuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<ChangeMasterDataRequest>, ChangeMasterDataRequestValidator>(Lifestyle.Scoped);
            _container.Register<IDomainEventsAccessor, DomainEventsAccessor>();
            _container.Register<IDomainEventsDispatcher, DomainEventsDispatcher>();
            _container.Register<IDomainEventPublisher, DomainEventPublisher>();
            _container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Singleton);
            _container.Register<ICommandScheduler, CommandScheduler>(Lifestyle.Scoped);
            _container.Register<IUserContext>(() => new UserContextStub { CurrentUser = new UserIdentity(Guid.NewGuid().ToString(), "8200000001409"), }, Lifestyle.Scoped);
            _container.Register<MeteringPointPipelineContext>(Lifestyle.Scoped);

            _container.Register<IDbConnectionFactory>(() => new SqlDbConnectionFactory(databaseFixture.DatabaseManager.ConnectionString), Lifestyle.Scoped);
            _container.Register<DbGridAreaHelper>(Lifestyle.Scoped);
            Dapper.SqlMapper.AddTypeHandler(NodaTimeSqlMapper.Instance);

            _container.Register<IBusinessProcessValidationContext, BusinessProcessValidationContext>(Lifestyle.Scoped);
            _container.Register<IBusinessProcessCommandFactory, BusinessProcessCommandFactory>(Lifestyle.Singleton);

            // TODO: remove this when infrastructure and application has been split into more assemblies.
            _container.Register<IDocumentSerializer<ConfirmMessage>, ConfirmMessageSerializer>(Lifestyle.Singleton);
            _container.Register<IDocumentSerializer<RejectMessage>, RejectMessageSerializer>(Lifestyle.Singleton);

            _container.Register<IActorMessageFactory, ActorMessageFactory>(Lifestyle.Scoped);
            _container.Register<IMessageHubDispatcher, MessageHubDispatcher>(Lifestyle.Scoped);
            _container.Register<IBusinessDocumentFactory, BusinessDocumentFactory>(Lifestyle.Scoped);

            _container.Register<ChangeMasterDataSettings>(() => new ChangeMasterDataSettings(NumberOfDaysEffectiveDateIsAllowedToBeforeToday: 1));

            _container.Register<IMeteringPointOwnershipProvider, MeteringPointOwnershipProvider>();

            _container.Register<IAuthorizer<ChangeMasterDataRequest>, Authorizer>();
            _container.Register<IAuthorizer<CreateMeteringPoint>, NullAuthorizer<CreateMeteringPoint>>();
            _container.Register<IAuthorizer<CreateGridArea>, NullAuthorizer<CreateGridArea>>();
            _container.Register<IAuthorizer<ConnectMeteringPoint>, NullAuthorizer<ConnectMeteringPoint>>();
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
                    typeof(CreateGridAreaHandler).Assembly,
                },
                new[]
                {
                    typeof(UnitOfWorkBehavior<,>),
                    typeof(AuthorizationBehavior<,>),
                    typeof(InputValidationBehavior<,>),
                    typeof(DomainEventsDispatcherBehaviour<,>),
                    typeof(InternalCommandHandlingBehaviour<,>),
                    typeof(BusinessProcessResultBehavior<,>),
                });

            // Specific for test instead of using Application Insights package
            _container.Register(() => new TelemetryClient(new TelemetryConfiguration()), Lifestyle.Scoped);

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

        protected async Task AssertAndRunInternalCommandAsync<TCommand>()
            where TCommand : ICommand
        {
            var meteringPointContext = GetService<MeteringPointContext>();
            var commands = meteringPointContext
                .QueuedInternalCommands
                .Where(c => c.ProcessedDate == null && c.Type == typeof(TCommand).FullName)
                .ToList();

            var messageExtractor = GetService<MessageExtractor>();

            foreach (var command in commands)
            {
                var message = await messageExtractor.ExtractAsync(command.Data).ConfigureAwait(false);

                // var meteringPointEnvelope = MeteringPointEnvelope.Parser.ParseFrom(command.Data);
                //     meteringPointEnvelope.SendAccountingPointCharacteristicsMessage.
                await SendCommandAsync(message).ConfigureAwait(false);
            }
        }

        protected void AssertOutboxMessage<TMessage>(Func<TMessage, bool> funcAssert, int count = 1)
        {
            if (funcAssert == null)
                throw new ArgumentNullException(nameof(funcAssert));

            var messages = GetOutboxMessages<TMessage>()
                .Where(funcAssert.Invoke)
                .ToList();

            messages.Should().NotBeNull();
            messages.Should().AllBeOfType<TMessage>();
            messages.Should().HaveCount(count);
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

        protected void AssertValidationError(string expectedErrorCode, bool expectError = true)
        {
            var message = GetOutboxMessages
                    <MessageHubEnvelope>()
                .SingleOrDefault(msg => msg.MessageType.Name.EndsWith("rejected", StringComparison.OrdinalIgnoreCase));

            if (message == null && expectError == false)
            {
                return;
            }

            if (message == null)
            {
                throw new XunitException("No message was found in outbox.");
            }

            var rejectMessage = GetService<IJsonSerializer>().Deserialize<RejectMessage>(message!.Content);

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

            if (expectError)
            {
                Assert.NotNull(validationError);
            }
            else
            {
                Assert.Null(validationError);
            }
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

        protected async Task AssertMeteringPointExistsAsync(string gsrnNumber)
        {
            if (gsrnNumber == null)
                throw new ArgumentNullException(nameof(gsrnNumber));
            Assert.NotNull(await GetService<IMeteringPointRepository>().GetByGsrnNumberAsync(GsrnNumber.Create(gsrnNumber)).ConfigureAwait(false));
        }

        protected void SetGridOperatorAsAuthenticatedUser(string glnNumber)
        {
            ((UserContextStub)GetService<IUserContext>()).SetCurrentUser(new UserIdentity("Fake", glnNumber));
        }

        private void CleanupDatabase()
        {
            var cleanupStatement = new StringBuilder();

            cleanupStatement.AppendLine($"DELETE FROM EnergySuppliers");
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
