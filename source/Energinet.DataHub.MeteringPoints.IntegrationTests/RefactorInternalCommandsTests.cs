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

using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using MediatR;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    [IntegrationTest]
    public class RefactorInternalCommandsTests : TestHost
    {
        public RefactorInternalCommandsTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Command_is_executed()
        {
            var unitOfWork = GetService<IUnitOfWork>();
            var accessor = new InternalCommandAccessor(GetService<MeteringPointContext>());
            var processor = new InternalCommandProcessor(
                accessor,
                GetService<ISystemDateTimeProvider>(),
                GetService<IJsonSerializer>(),
                GetService<IMediator>());
            var scheduler = new CommandScheduler(
                GetService<MeteringPointContext>(),
                GetService<IJsonSerializer>(),
                GetService<ISystemDateTimeProvider>(),
                GetService<ICorrelationContext>());
            await scheduler.EnqueueAsync(new TestCommand()).ConfigureAwait(false);
            await unitOfWork.CommitAsync().ConfigureAwait(false);

            await processor.ProcessUndispatchedAsync().ConfigureAwait(false);

            var pending = await accessor.GetPendingAsync().ConfigureAwait(false);

            Assert.Empty(pending);
        }
    }
}
