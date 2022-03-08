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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.EntryPoints.Processing;
using Energinet.DataHub.MeteringPoints.EntryPoints.WebApi.MeteringPoints.Queries;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using SimpleInjector;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CreateMeteringPoints
{
    [IntegrationTest]
    public class CreateFooTests
        : EntryPointBasedFixture<Program>
    {
        public CreateFooTests(DatabaseFixture databaseFixture)
            : base(databaseFixture) { }

        [Fact]
        public async Task Metering_point_created_shows_in_process_overview()
        {
            var request = CreateCommand();

            await Act.SendCommandAsync(request).ConfigureAwait(false);

            await Assert.ProcessOverviewAsync(
                    SampleData.GsrnNumber,
                    "BRS-004",
                    "RequestCreateMeteringPoint",
                    "ConfirmCreateMeteringPoint")
                .ConfigureAwait(false);
        }

        protected override void AddEnvironmentVariables([NotNull] Dictionary<string, string> variables)
        {
            variables.Add("METERINGPOINT_DB_CONNECTION_STRING", DatabaseConnectionString);
        }

        protected override void OverrideRegistrations([NotNull] Container container)
        {
            // Fake current actor and user
            container.Register<IActorContext>(() => new ActorContext { CurrentActor = new Actor(SampleData.GridOperatorIdOfGrid870, "GLN", "8200000001409", "GridAccessProvider") }, Lifestyle.Singleton);
            container.Register<IUserContext>(() => new UserContext { CurrentUser = new User(Guid.NewGuid(), new List<Guid> { Guid.NewGuid() }) }, Lifestyle.Singleton);

            // Singleton for tests
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Singleton);

            // Specific for test instead of using Application Insights package
            container.Register(() => new TelemetryClient(new TelemetryConfiguration()), Lifestyle.Scoped);

            // Only for asserting process overview creation
            container.Register<IRequestHandler<MeteringPointProcessesByGsrnQuery, List<Process>>, MeteringPointProcessesByGsrnQueryHandler>();
        }

        private static CreateMeteringPoint CreateCommand()
        {
            return Scenarios.CreateConsumptionMeteringPointCommand();
        }
    }
}
