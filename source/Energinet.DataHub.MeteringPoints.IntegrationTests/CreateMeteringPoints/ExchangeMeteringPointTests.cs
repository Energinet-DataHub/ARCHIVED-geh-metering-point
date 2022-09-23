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

using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint;
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
            AssertConfirmMessage(DocumentType.ConfirmCreateMeteringPoint, "E02");
            var integrationEvent = FindIntegrationEvent<MeteringPointCreatedEventMessage>();
            Assert.NotNull(integrationEvent);
            Assert.Equal(request.GsrnNumber, integrationEvent?.GsrnNumber);
            Assert.Equal(request.MeteringMethod, integrationEvent?.MeteringMethod);
            Assert.Equal(request.MeterReadingOccurrence, integrationEvent?.MeterReadingPeriodicity);
            Assert.Equal(request.MeterReadingOccurrence, integrationEvent?.MeterReadingPeriodicity);
            Assert.NotNull(integrationEvent?.GridOperatorId);
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

            AssertValidationError("E86", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_municipality_code_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    MunicipalityCode = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_street_code_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    StreetCode = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_building_number_is_missing()
        {
            var request = CreateCommand()
                with
                {
                    BuildingNumber = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("E86", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_from_grid_is_not_existing()
        {
            var request = CreateCommand()
                with
                {
                    ExchangeDetails = new ExchangeDetails(GridAreaCode.Create("111").Value, GridAreaCode.Create("870").Value),
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D46", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Should_reject_when_to_grid_is_not_existing()
        {
            var request = CreateCommand()
                with
                {
                    ExchangeDetails = new ExchangeDetails(SampleData.MeteringGridArea, GridAreaCode.Create("111").Value),
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D46", DocumentType.RejectCreateMeteringPoint);
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

            AssertValidationError("E86", DocumentType.RejectCreateMeteringPoint);
        }

        [Fact]
        public async Task Exchange_metering_point_should_contain_disconnection_type()
        {
            var request = Scenarios.CreateCommand(MeteringPointType.Exchange)
                with
                {
                    MeteringMethod = MeteringMethod.Physical.Name,
                    DisconnectionType = null,
                    MeterNumber = "pva30909290",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D02");
        }

        private static CreateMeteringPoint CreateCommand()
        {
            return Scenarios.CreateCommand(MeteringPointType.Exchange);
        }
    }
}
