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
using Energinet.DataHub.MeteringPoints.Application.Common.Users;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Application.Validation;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Pipeline;
using Energinet.DataHub.MeteringPoints.Infrastructure.ContainerExtensions;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.UserIdentity;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using FluentAssertions;
using FluentValidation;
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
using ErrorMessage = Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors.ErrorMessage;
using JsonSerializer = Energinet.DataHub.MeteringPoints.Infrastructure.Serialization.JsonSerializer;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.MarketDocuments
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

            serviceCollection.AddDbContext<MeteringPointContext>(
                x =>
                x.UseSqlServer(connectionString, y => y.UseNodaTime()),
                ServiceLifetime.Scoped);
            serviceCollection.AddSimpleInjector(_container);
            _serviceProvider = serviceCollection.BuildServiceProvider().UseSimpleInjector(_container);

            _container.Register<IUserContext, UserContext>(Lifestyle.Scoped);
            _container.Register<IUnitOfWork, UnitOfWork>(Lifestyle.Scoped);
            _container.Register<IOutbox, OutboxProvider>(Lifestyle.Scoped);
            _container.Register<IOutboxManager, OutboxManager>(Lifestyle.Scoped);
            _container.Register<IOutboxMessageFactory, OutboxMessageFactory>(Lifestyle.Singleton);
            _container.Register<IJsonSerializer, JsonSerializer>(Lifestyle.Singleton);
            _container.Register<ISystemDateTimeProvider, SystemDateTimeProviderStub>(Lifestyle.Singleton);
            _container.Register<IValidator<MasterDataDocument>, ValidationRuleSet>(Lifestyle.Scoped);
            _container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Singleton);
            _container.Register<IBusinessProcessValidationContext, BusinessProcessValidationContext>(Lifestyle.Scoped);
            _container.Register<IBusinessProcessCommandFactory, TestBusinessProcessCommandFactory>(Lifestyle.Singleton);
            _container.Register(typeof(IBusinessProcessResultHandler<TestBusinessRequest>), typeof(TestBusinessRequestResultHandler), Lifestyle.Scoped);

            _container.AddValidationErrorConversion(
                validateRegistrations: true,
                typeof(MasterDataDocument).Assembly, // Application
                typeof(MeteringPoint).Assembly, // Domain
                typeof(ErrorMessageFactory).Assembly); // Infrastructure

            var testAssembly = typeof(TestBusinessRequest).Assembly;
            _container.RegisterSingleton<IMediator, Mediator>();
            _container.Register(typeof(IRequestHandler<MasterDataDocument, BusinessProcessResult>), typeof(MasterDataDocumentHandler));
            _container.Register(typeof(IRequestHandler<TestBusinessRequest, BusinessProcessResult>), typeof(TestBusinessRequestHandler));

            var pipelineBehaviors = new[]
            {
                typeof(UnitOfWorkBehavior<,>),
                typeof(InputValidationBehavior<,>),
                typeof(BusinessProcessResultBehavior<,>),
            };
            _container.Collection.Register(typeof(IPipelineBehavior<,>), pipelineBehaviors);
            _container.Register(() => new ServiceFactory(_container.GetInstance), Lifestyle.Singleton);

            _container.Register<IValidator<TestBusinessRequest>, NullValidationSet<TestBusinessRequest>>(Lifestyle.Scoped);
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

            _container.GetInstance<MeteringPointContext>()
                .Database.ExecuteSqlRaw(cleanupStatement.ToString());
        }
    }
}
