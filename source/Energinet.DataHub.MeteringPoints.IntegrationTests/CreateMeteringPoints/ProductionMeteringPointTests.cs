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
using Energinet.DataHub.MeteringPoints.Application.Create.Production;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Production;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CreateMeteringPoints
{
    public class ProductionMeteringPointTests
        : TestHost
    {
        public ProductionMeteringPointTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Metering_point_is_created()
        {
            var request = CreateCommand();
            await SendCommandAsync(request).ConfigureAwait(false);

            AssertConfirmMessage(DocumentType.CreateMeteringPointAccepted);
            await AssertMeteringPointExistsAsync(request.GsrnNumber).ConfigureAwait(false);
            var integrationEvent = FindIntegrationEvent<ProductionMeteringPointCreatedIntegrationEvent>();
            Assert.NotNull(integrationEvent);
            Assert.Equal(request.GsrnNumber, integrationEvent?.GsrnNumber);
            Assert.Equal(request.MeteringMethod, integrationEvent?.MeteringMethod);
            Assert.Equal(request.MeteringGridArea, integrationEvent?.GridAreaCode);
            Assert.Equal(request.MeterReadingOccurrence, integrationEvent?.MeterReadingPeriodicity);
            Assert.Equal(request.NetSettlementGroup, integrationEvent?.NetSettlementGroup);
        }

        private static CreateProductionMeteringPoint CreateCommand()
        {
            return Scenarios.CreateProductionMeteringPointCommand();
        }
    }
}
