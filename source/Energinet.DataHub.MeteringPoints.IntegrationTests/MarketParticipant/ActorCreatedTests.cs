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
using Dapper;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Application.MarketParticipants.ActorsCreated;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using JetBrains.Annotations;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.MarketParticipant;

public class ActorCreatedTests : TestHost
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly InternalCommandProcessor _processor;
    private readonly ISystemDateTimeProvider _timeProvider;
    private readonly ICommandScheduler _scheduler;
    private readonly IDbConnectionFactory _connectionFactory;

    public ActorCreatedTests([NotNull] DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
        _unitOfWork = GetService<IUnitOfWork>();
        _processor = GetService<InternalCommandProcessor>();
        _timeProvider = GetService<ISystemDateTimeProvider>();
        _scheduler = GetService<ICommandScheduler>();
        _connectionFactory = GetService<IDbConnectionFactory>();
    }

    [Fact]
    public async Task New_actor_is_created()
    {
        await EventIsReceivedAndProcessed().ConfigureAwait(false);

        var actor = await GetActor().ConfigureAwait(false);

        Assert.NotNull(actor);
        Assert.Equal(Guid.Parse(SampleData.ActorId), actor!.Id);
    }

    private static ActorCreated CreateCommand()
    {
        return new ActorCreated(
            Guid.Parse(SampleData.ActorId),
            SampleData.GlnNumber,
            "GLN");
    }

    private async Task<Actor?> GetActor()
    {
        var sql = $"SELECT Id FROM [dbo].[Actor] WHERE Id = '{SampleData.ActorId}'";
        return await _connectionFactory.GetOpenConnection().QuerySingleOrDefaultAsync<Actor>(sql).ConfigureAwait(false);
    }

    private async Task EventIsReceivedAndProcessed()
    {
        var command = CreateCommand();
        await Schedule(command).ConfigureAwait(false);
        await ProcessPendingCommands().ConfigureAwait(false);
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

    #pragma warning disable
    public record Actor(Guid Id);
    #pragma warning restore
}
