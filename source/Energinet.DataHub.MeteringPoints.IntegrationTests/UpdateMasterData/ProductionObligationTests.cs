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
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData.ConsumptionMeteringPoints;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData
{
    [IntegrationTest]
    public class ProductionObligationTests
        : TestHost
    {
        private readonly ISystemDateTimeProvider _timeProvider;

        public ProductionObligationTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _timeProvider = GetService<ISystemDateTimeProvider>();
        }

        [Fact]
        public async Task Production_obligation_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateProductionMeteringPointCommand()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ProductionObligation = true,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasProductionObligation(true);
        }

        private AssertPersistedMeteringPoint AssertMasterData()
        {
            return AssertPersistedMeteringPoint
                .Initialize(SampleData.GsrnNumber, GetService<IDbConnectionFactory>());
        }
    }
}
