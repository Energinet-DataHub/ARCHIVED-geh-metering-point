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

using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Create.Exchange;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Exchange;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.CreateMeteringPoints
{
    [IntegrationTest]
    public class ExchangeMeteringPointTests
        : TestHost
    {
        public ExchangeMeteringPointTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Metering_point_is_created()
        {
            var request = CreateCommand();
            await SendCommandAsync(request).ConfigureAwait(false);

            await AssertMeteringPointExistsAsync(request.GsrnNumber).ConfigureAwait(false);
            AssertConfirmMessage(DocumentType.CreateMeteringPointAccepted);
            var integrationEvent = FindIntegrationEvent<ExchangeMeteringPointCreatedIntegrationEvent>();
            Assert.NotNull(integrationEvent);
            Assert.Equal(request.GsrnNumber, integrationEvent?.GsrnNumber);
            Assert.Equal(request.MeteringMethod, integrationEvent?.MeteringMethod);
            Assert.Equal(request.MeteringGridArea, integrationEvent?.GridAreaCode);
            Assert.Equal(request.MeterReadingOccurrence, integrationEvent?.MeterReadingPeriodicity);
            Assert.Equal(request.FromGrid, integrationEvent?.FromGrid);
            Assert.Equal(request.ToGrid, integrationEvent?.ToGrid);
            Assert.Equal(request.MeterReadingOccurrence, integrationEvent?.MeterReadingPeriodicity);
        }

        [Fact]
        public async Task Should_reject_when_street_name_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    StreetName = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_when_from_grid_is_not_existing()
        {
            var request = CreateCommand()
                with
                {
                    FromGrid = GridAreaCode.Create("111").Value,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D46", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_when_to_grid_is_not_existing()
        {
            var request = CreateCommand()
                with
                {
                    FromGrid = SampleData.MeteringGridArea,
                    ToGrid = GridAreaCode.Create("111").Value,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D46", DocumentType.CreateMeteringPointRejected);
        }

        [Fact]
        public async Task Should_reject_if_geo_info_reference_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    GeoInfoReference = null,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.CreateMeteringPointRejected);
        }

        private static CreateExchangeMeteringPoint CreateCommand()
        {
            return Scenarios.CreateExchangeMeteringPointCommand();
        }
    }
}
