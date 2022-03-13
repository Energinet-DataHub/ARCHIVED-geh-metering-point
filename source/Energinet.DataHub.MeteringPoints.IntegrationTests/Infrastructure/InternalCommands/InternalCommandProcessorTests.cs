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

using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.Infrastructure.InternalCommands
{
    [IntegrationTest]
    public class InternalCommandProcessorTests : TestHost
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly InternalCommandProcessor _processor;
        private readonly ISystemDateTimeProvider _timeProvider;
        private readonly ICommandScheduler _scheduler;

        public InternalCommandProcessorTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _unitOfWork = GetService<IUnitOfWork>();
            _processor = GetService<InternalCommandProcessor>();
            _timeProvider = GetService<ISystemDateTimeProvider>();
            _scheduler = GetService<ICommandScheduler>();
        }

        [Fact]
        public async Task When_execution_fails_the_exception_is_logged_and_command_is_marked_as_processed()
        {
            var commandThatThrows = new TestCommand(throwException: true);
            await Schedule(commandThatThrows).ConfigureAwait(false);

            await ProcessPendingCommands().ConfigureAwait(false);

            AssertIsNotProcessed(commandThatThrows);
            AssertHasException(commandThatThrows);
        }

        [Fact]
        public async Task Continue_processing_even_if_an_exception_is_thrown()
        {
            var commandThatThrows = new TestCommand(throwException: true);
            var command = new TestCommand();
            await Schedule(commandThatThrows).ConfigureAwait(false);
            await Schedule(command).ConfigureAwait(false);

            await ProcessPendingCommands().ConfigureAwait(false);

            AssertIsProcessed(command);
            AssertIsNotProcessed(commandThatThrows);
        }

        [Fact]
        public async Task Scheduled_commands_are_processed()
        {
            var yesterday = _timeProvider.Now().Minus(Duration.FromDays(1));
            var command = new TestCommand();
            await Schedule(command, yesterday).ConfigureAwait(false);

            await ProcessPendingCommands().ConfigureAwait(false);

            AssertIsProcessed(command);
        }

        [Fact]
        public async Task Commands_scheduled_to_be_run_in_the_future_are_not_processed()
        {
            var tomorrow = _timeProvider.Now().Plus(Duration.FromDays(1));
            var command = new TestCommand();
            await Schedule(command, tomorrow).ConfigureAwait(false);

            await ProcessPendingCommands().ConfigureAwait(false);

            AssertIsNotProcessed(command);
        }

        private void AssertIsProcessed(InternalCommand command)
        {
            var checkStatement =
                $"SELECT COUNT(1) FROM [dbo].[QueuedInternalCommands] WHERE Id = '{command.Id}' AND ProcessedDate IS NOT NULL";
            var isProcessed = GetService<IDbConnectionFactory>().GetOpenConnection().ExecuteScalar<bool>(checkStatement);
            Assert.True(isProcessed);
        }

        private void AssertHasException(InternalCommand command)
        {
            var checkStatement =
                $"SELECT COUNT(1) FROM [dbo].[QueuedInternalCommands] WHERE Id = '{command.Id}' AND [Error] IS NOT NULL";
            var hasException = GetService<IDbConnectionFactory>().GetOpenConnection().ExecuteScalar<bool>(checkStatement);
            Assert.True(hasException);
        }

        private void AssertIsNotProcessed(InternalCommand command)
        {
            var queuedInternalCommand = GetQueuedCommandFrom(command);
            Assert.Null(queuedInternalCommand.ProcessedDate);
        }

        private QueuedInternalCommand GetQueuedCommandFrom(InternalCommand command)
        {
            var context = GetService<MeteringPointContext>();
            var queuedInternalCommand = context.QueuedInternalCommands.First(x => x.Id.Equals(command.Id));
            return queuedInternalCommand;
        }

        private async Task ProcessPendingCommands()
        {
            await _processor.ProcessPendingAsync().ConfigureAwait(false);
        }

        private async Task Schedule(InternalCommand command, Instant? executeOn = null)
        {
            await _scheduler.EnqueueAsync(command, executeOn).ConfigureAwait(false);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
    }
}
