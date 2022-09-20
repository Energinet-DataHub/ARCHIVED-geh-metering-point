﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.ChangeConnectionStatus;
using Energinet.DataHub.MeteringPoints.Application.CloseDown;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.ChildMeteringPoints;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Application.Common.DomainEvents;
using Energinet.DataHub.MeteringPoints.Application.Common.Queries;
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.Create.Validation;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Application.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Application.ProcessOverview;
using Energinet.DataHub.MeteringPoints.Application.Providers.MeteringPointOwnership;
using Energinet.DataHub.MeteringPoints.Application.RequestMasterData;
using Energinet.DataHub.MeteringPoints.Application.UpdateMasterData;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses.UpdateMasterData;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.WebApi;
using Energinet.DataHub.MeteringPoints.EntryPoints.WebApi.MeteringPoints.Queries;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Authorization;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Pipeline;
using Energinet.DataHub.MeteringPoints.Infrastructure.ContainerExtensions;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.EnergySuppliers.Queries;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.GridAreas;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MessageHub.Bundling;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints.Queries;
using Energinet.DataHub.MeteringPoints.Infrastructure.DomainEventDispatching;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ChangeConnectionStatus;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ChangeMasterData;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.ConnectMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.GenericNotification;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.ChargeLinks.Create;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeConnectionStatus.Disconnect;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeConnectionStatus.Reconnect;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeMasterData.MasterDataUpdated;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.Connect;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint;
using Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Providers.MeteringPointOwnership;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Infrastructure.InternalCommands;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Confirm;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Generic;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Reject;
using Energinet.DataHub.MeteringPoints.RequestResponse.Contract;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;
using Xunit.Categories;
using Xunit.Sdk;
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
            _container.Register<IBusinessProcessRepository, BusinessProcessRepository>(Lifestyle.Scoped);
            _container.Register<IOutbox, OutboxProvider>(Lifestyle.Scoped);
            _container.Register<IOutboxManager, OutboxManager>(Lifestyle.Scoped);
            _container.Register<IOutboxMessageFactory, OutboxMessageFactory>(Lifestyle.Singleton);
            _container.Register<IJsonSerializer, JsonSerializer>(Lifestyle.Singleton);
            _container.Register<ISystemDateTimeProvider, RunnableDateTimeProviderStub>(Lifestyle.Singleton);
            _container.Register(typeof(IBusinessProcessResultHandler<UpdateMasterDataRequest>), typeof(ChangeMasterDataResultHandler), Lifestyle.Scoped);
            _container.Register(typeof(IBusinessProcessResultHandler<CreateMeteringPoint>), typeof(CreateMeteringPointResultHandler<CreateMeteringPoint>), Lifestyle.Scoped);
            _container.Register(typeof(IBusinessProcessResultHandler<ConnectMeteringPointRequest>), typeof(ConnectMeteringPointResultHandler), Lifestyle.Scoped);
            _container.Register(typeof(IBusinessProcessResultHandler<DisconnectReconnectMeteringPointRequest>), typeof(DisconnectReconnectMeteringPointResultHandler), Lifestyle.Scoped);
            _container.Register<IValidator<MasterDataDocument>, ValidationRuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<ConnectMeteringPointRequest>, ConnectMeteringPointRuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<DisconnectReconnectMeteringPointRequest>, DisconnectReconnectMeteringPointRuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<CreateMeteringPoint>, RuleSet>(Lifestyle.Scoped);
            _container.Register<IValidator<UpdateMasterDataRequest>, UpdateMasterDataRequestValidator>(Lifestyle.Scoped);
            _container.Register<RequestCloseDownValidator>(Lifestyle.Scoped);
            _container.Register<IDomainEventsAccessor, DomainEventsAccessor>();
            _container.Register<IDomainEventsDispatcher, DomainEventsDispatcher>();
            _container.Register<IDomainEventPublisher, DomainEventPublisher>();
            _container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Singleton);
            _container.Register<ICommandScheduler, CommandScheduler>(Lifestyle.Scoped);
            _container.Register<InternalCommandProcessor>(Lifestyle.Scoped);
            _container.Register<InternalCommandAccessor>(Lifestyle.Scoped);
            _container.Register<CommandExecutor>(Lifestyle.Scoped);
            _container.Register<IActorContext>(() => new ActorContext { CurrentActor = new Actor(SampleData.GridOperatorIdOfGrid870, "GLN", "8200000001409", "GridAccessProvider") }, Lifestyle.Singleton);
            _container.Register<IUserContext>(() => new UserContext { CurrentUser = new User(Guid.NewGuid(), new List<Guid> { Guid.NewGuid() }) }, Lifestyle.Singleton);
            _container.Register<MeteringPointPipelineContext>(Lifestyle.Scoped);
            _container.Register<IActorProvider, ActorProvider>(Lifestyle.Scoped);
            _container.Register<IUserProvider, UserProvider>(Lifestyle.Scoped);
            _container.Register<IDbConnectionFactory>(() => new SqlDbConnectionFactory(databaseFixture.DatabaseManager.ConnectionString), Lifestyle.Scoped);
            _container.Register<DbGridAreaHelper>(Lifestyle.Scoped);
            _container.Register<ProtobufOutboundMapperFactory>();
            _container.Register<ProtobufInboundMapperFactory>();
            Dapper.SqlMapper.AddTypeHandler(NodaTimeSqlMapper.Instance);

            _container.Register<IBusinessProcessValidationContext, BusinessProcessValidationContext>(Lifestyle.Scoped);
            _container.Register<IBusinessProcessCommandFactory, BusinessProcessCommandFactory>(Lifestyle.Singleton);

            // TODO: remove this when infrastructure and application has been split into more assemblies.
            _container.Register<IDocumentSerializer<ConfirmMessage>, ConfirmMessageXmlSerializer>(Lifestyle.Singleton);
            _container.Register<IDocumentSerializer<RejectMessage>, RejectMessageXmlSerializer>(Lifestyle.Singleton);
            _container.Register<IDocumentSerializer<GenericNotificationMessage>, GenericNotificationMessageXmlSerializer>(Lifestyle.Singleton);
            _container.Register<IDocumentSerializer<AccountingPointCharacteristicsMessage>, AccountingPointCharacteristicsMessageXmlSerializer>(Lifestyle.Singleton);

            _container.Register<IMessageHubDispatcher, MessageHubDispatcher>(Lifestyle.Scoped);
            _container.Register<IActorMessageService, ActorMessageService>(Lifestyle.Scoped);

            _container.Register<PolicyThresholds>(() => new PolicyThresholds(NumberOfDaysEffectiveDateIsAllowedToBeforeToday: 1));
            _container.Register<ConnectSettings>(() => new ConnectSettings(
                NumberOfDaysEffectiveDateIsAllowedToBeforeToday: 7,
                NumberOfDaysEffectiveDateIsAllowedToAfterToday: 0));
            _container.Register<DisconnectReconnectSettings>(() => new DisconnectReconnectSettings(
                1,
                0));

            _container.AddMasterDataValidators(typeof(IMasterDataValidatorStrategy).Assembly);
            _container.AddMasterDataUpdateServices();
            _container.Register<ParentCouplingService>(Lifestyle.Scoped);

            _container.Register<IMeteringPointOwnershipProvider, MeteringPointOwnershipProvider>();
            _container.AddBusinessProcessAuthorizers();

            _container.AddBusinessRequestReceivers();

            _container.UseMediatR()
                .WithPipeline(
                    typeof(UnitOfWorkBehavior<,>),
                    typeof(ProcessOverviewBehavior<,>),
                    typeof(InputValidationBehavior<,>),
                    typeof(DomainEventsDispatcherBehaviour<,>),
                    typeof(InternalCommandHandlingBehaviour<,>),
                    typeof(BusinessProcessResultBehavior<,>))
                .WithRequestHandlers(
                    typeof(UpdateMasterDataHandler),
                    typeof(MasterDataDocumentHandler),
                    typeof(DisconnectReconnectMeteringPointHandler),
                    typeof(CreateMeteringPointHandler),
                    typeof(AddEnergySupplierHandler),
                    typeof(ConnectMeteringPointHandler),
                    typeof(SendAccountingPointCharacteristicsMessageHandler),
                    typeof(SetEnergySupplierDetailsHandler),
                    typeof(CreateDefaultChargeLinksHandler),
                    typeof(MeteringPointByIdQueryHandler),
                    typeof(MeteringPointGsrnExistsQueryHandler),
                    typeof(EnergySuppliersByMeteringPointIdQueryHandler),
                    typeof(MeteringPointByGsrnQueryHandler),
                    typeof(CloseDownMeteringPointHandler),
                    typeof(TestCommandHandler),
                    typeof(GetMasterDataQueryHandler))
                .WithNotificationHandlers(
                    typeof(MeteringPointCreatedNotificationHandler),
                    typeof(OnMeteringPointConnected),
                    typeof(OnMeteringPointDisconnected),
                    typeof(OnMeteringPointReconnected),
                    typeof(OnMasterDataWasUpdated),
                    typeof(SetEnergySupplierHACK),
                    typeof(ProcessInternalCommandsOnTimeHasPassed));

            // Specific for test instead of using Application Insights package
            _container.Register(() => new TelemetryClient(new TelemetryConfiguration()), Lifestyle.Scoped);

            // Only for asserting process overview creation
            _container.Register<IRequestHandler<MeteringPointProcessesByGsrnQuery, List<Process>>, MeteringPointProcessesByGsrnQueryHandler>();

            _container.Register(typeof(ProcessExtractor<>), typeof(CreateMeteringPointProcessExtractor), Lifestyle.Scoped);
            _container.Register(typeof(ProcessExtractor<>), typeof(ConnectMeteringPointProcessExtractor), Lifestyle.Scoped);
            _container.Register(typeof(ProcessExtractor<>), typeof(UpdateMeteringPointProcessExtractor), Lifestyle.Scoped);
            _container.Register(typeof(ProcessExtractor<>), typeof(DisconnectReconnectMeteringPointProcessExtractor), Lifestyle.Scoped);
            _container.RegisterConditional(typeof(ProcessExtractor<>), typeof(NullProcessExtractor<>), context => !context.Handled);

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

        protected async Task<TCommandType?> GetScheduledCommandAsync<TCommandType>(Instant withScheduleDateAtOrBefore)
            where TCommandType : InternalCommand
        {
            var accessor = GetService<InternalCommandAccessor>();
            var queuedCommands = await accessor.GetPendingAsync(withScheduleDateAtOrBefore).ConfigureAwait(false);
            return queuedCommands
                .Select(queuedCommand => queuedCommand.ToCommand(GetService<IJsonSerializer>()))
                .First(queuedCommand => queuedCommand is TCommandType) as TCommandType;
        }

        protected async Task AssertAndRunInternalCommandAsync<TCommand>()
            where TCommand : ICommand<Unit>
        {
            var meteringPointContext = GetService<MeteringPointContext>();
            var commands = meteringPointContext
                .QueuedInternalCommands
                .Where(c => c.ProcessedDate == null && c.Type == typeof(TCommand).AssemblyQualifiedName)
                .ToList();

            var serializer = GetService<IJsonSerializer>();
            foreach (var command in commands)
            {
                var message = serializer.Deserialize(command.Data, Type.GetType(command.Type, true)!);
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

        protected TMessage? AssertOutboxMessageAndReturnMessage<TMessage>()
        {
            var message = GetOutboxMessages<TMessage>().SingleOrDefault();

            message.Should().NotBeNull();
            message.Should().BeOfType<TMessage>();

            return message;
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
                errorMessage.AppendLine(CultureInfo.InvariantCulture, $"Reject message contains more ({errorCount}) than 1 error:");
                foreach (var error in rejectMessage.MarketActivityRecord.Reasons)
                {
                    errorMessage.AppendLine(CultureInfo.InvariantCulture, $"Code: {error.Code}. Description: {error.Text}.");
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
                .SingleOrDefault(msg => msg.MessageType.Name.StartsWith("Reject", StringComparison.OrdinalIgnoreCase));

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
                errorMessage.AppendLine(CultureInfo.InvariantCulture, $"Reject message contains more ({errorCount}) than 1 error:");
                foreach (var error in rejectMessage.MarketActivityRecord.Reasons)
                {
                    errorMessage.AppendLine(CultureInfo.InvariantCulture, $"Code: {error.Code}. Description: {error.Text}.");
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

        protected void AssertConfirmMessage(DocumentType documentType, string businessProcess)
        {
            var message = GetOutboxMessages
                    <MessageHubEnvelope>()
                .FirstOrDefault(msg => msg.MessageType.Equals(documentType));

            var confirmMessage = GetService<IJsonSerializer>().Deserialize<ConfirmMessage>(message!.Content);

            Assert.NotNull(confirmMessage);
            Assert.Equal(businessProcess, confirmMessage.ProcessType);
        }

        protected void AssertRejectMessage(DocumentType documentType, string businessProcess)
        {
            var message = GetOutboxMessages
                    <MessageHubEnvelope>()
                .FirstOrDefault(msg => msg.MessageType.Equals(documentType));

            var rejectMessage = GetService<IJsonSerializer>().Deserialize<RejectMessage>(message!.Content);

            Assert.NotNull(rejectMessage);
            Assert.Equal(businessProcess, rejectMessage.ProcessType);
        }

        protected async Task AssertMultipleProcessOverviewAsync(
            string gsrn,
            string expectedProcessName,
            int expectedProcessCount,
            params string[] expectedProcessSteps)
        {
            var processes = (await GetService<IRequestHandler<MeteringPointProcessesByGsrnQuery, List<Process>>>()
                .Handle(new MeteringPointProcessesByGsrnQuery(gsrn), CancellationToken.None)
                .ConfigureAwait(false)).Where(process => process.Name == expectedProcessName).ToList();

            processes.Should()
                .HaveCount(expectedProcessCount)
                .And
                .AllSatisfy(process => process
                    .Details.Select(detail => detail.Name)
                    .Should()
                    .ContainInOrder(expectedProcessSteps));
        }

        protected async Task AssertProcessOverviewAsync(
            string gsrn,
            string expectedProcessName,
            params string[] expectedProcessSteps)
        {
            var processes = await GetService<IRequestHandler<MeteringPointProcessesByGsrnQuery, List<Process>>>()
                .Handle(new MeteringPointProcessesByGsrnQuery(gsrn), CancellationToken.None)
                .ConfigureAwait(false);

            processes.Should().ContainSingle(process => process.Name == expectedProcessName, $"a single process with name {expectedProcessName} was expected")
                .Which.Details.Select(detail => detail.Name).Should().ContainInOrder(expectedProcessSteps);
        }

        protected async Task SendCommandAsync(object command, CancellationToken cancellationToken = default)
        {
            await using var scope = AsyncScopedLifestyle.BeginScope(_container);
            await scope.GetInstance<IMediator>().Send(command, cancellationToken).ConfigureAwait(false);
        }

        protected async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query)
        {
            await using var scope = AsyncScopedLifestyle.BeginScope(_container);
            return await scope.GetInstance<IMediator>().Send(query, CancellationToken.None).ConfigureAwait(false);
        }

        protected async Task AssertMeteringPointExistsAsync(string gsrnNumber)
        {
            if (gsrnNumber == null)
                throw new ArgumentNullException(nameof(gsrnNumber));
            Assert.NotNull(await GetService<IMeteringPointRepository>().GetByGsrnNumberAsync(GsrnNumber.Create(gsrnNumber)).ConfigureAwait(false));
        }

        protected void SetGridOperatorAsAuthenticatedUser(string glnNumber)
        {
            ((ActorContext)GetService<IActorContext>()).CurrentActor = new Actor(Guid.NewGuid(), "GLN", glnNumber, "GridAccessProvider");
        }

        protected void SetCurrentAuthenticatedActor(Guid actorId)
        {
            ((ActorContext)GetService<IActorContext>()).CurrentActor = new Actor(actorId, "GLN", "FakeIdentifier", "GridAccessProvider");
        }

        protected EffectiveDate CreateEffectiveDateAsOfToday()
        {
            var today = GetService<ISystemDateTimeProvider>().Now().ToDateTimeUtc();
            return EffectiveDate.Create(TestHelpers.DaylightSavingsString(new DateTime(
                today.Year,
                today.Month,
                today.Day)));
        }

        protected async Task CreatePhysicalConsumptionMeteringPointAsync()
        {
            var request = Scenarios.CreateConsumptionMeteringPointCommand()
                with
            {
                MeteringMethod = MeteringMethod.Physical.Name,
                MeterNumber = "1",
                NetSettlementGroup = NetSettlementGroup.Zero.Name,
                ConnectionType = null,
                ScheduledMeterReadingDate = null,
            };
            await SendCommandAsync(request).ConfigureAwait(false);
        }

        protected Task CloseDownMeteringPointAsync()
        {
            return CloseDownMeteringPointAsync(SampleData.GsrnNumber);
        }

        protected async Task CloseDownMeteringPointAsync(string gsrnNumber)
        {
            var context = GetService<MeteringPointContext>();
            var meteringPoint = context.MeteringPoints.First(meteringPoint => meteringPoint.GsrnNumber.Equals(GsrnNumber.Create(gsrnNumber)));
            meteringPoint?.CloseDown();
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        protected MasterDataDocument CreateUpdateRequest()
        {
            return TestUtils.CreateRequest()
                with
            {
                TransactionId = SampleData.Transaction,
                GsrnNumber = SampleData.GsrnNumber,
                EffectiveDate = CreateEffectiveDateAsOfToday().ToString(),
            };
        }

        protected async Task CreateConsumptionMeteringPointInNetSettlementGroup6Async()
        {
            var request = Scenarios.CreateConsumptionMeteringPointCommand()
                with
            {
                EffectiveDate = CreateEffectiveDateAsOfToday().ToString(),
                MeteringMethod = MeteringMethod.Virtual.Name,
                NetSettlementGroup = NetSettlementGroup.Six.Name,
                ConnectionType = ConnectionType.Installation.Name,
                ScheduledMeterReadingDate = "0101",
            };
            await SendCommandAsync(request).ConfigureAwait(false);
        }

        protected AssertPersistedMeteringPoint AssertMasterData()
        {
            return AssertPersistedMeteringPoint
                .Initialize(SampleData.GsrnNumber, GetService<IDbConnectionFactory>());
        }

        protected AssertPersistedMeteringPoint AssertMasterData(string gsrnNumber)
        {
            return AssertPersistedMeteringPoint
                .Initialize(gsrnNumber, GetService<IDbConnectionFactory>());
        }

        protected AssertBusinessProcess AssertProcess()
        {
            return AssertBusinessProcess
                .Initialize(SampleData.Transaction, GetService<IDbConnectionFactory>());
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
            cleanupStatement.AppendLine($"DELETE FROM BusinessProcesses");

            _container.GetInstance<MeteringPointContext>()
                .Database.ExecuteSqlRaw(cleanupStatement.ToString());
        }
    }
}
