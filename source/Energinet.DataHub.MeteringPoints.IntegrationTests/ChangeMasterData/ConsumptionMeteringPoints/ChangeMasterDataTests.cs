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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.ChangeMasterData;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeMasterData;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.ChangeMasterData.ConsumptionMeteringPoints
{
    public class ChangeMasterDataTests : TestHost
    {
        public ChangeMasterDataTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Should_change_address()
        {
            await CreateMeteringPointAsync().ConfigureAwait(false);
            var request = new ChangeMasterDataRequest()
                with
                {
                    TransactionId = SampleData.Transaction,
                    GsrnNumber = SampleData.GsrnNumber,
                    StreetName = "New Street Name",
                    PostCode = "6000",
                    City = "New City Name",
                    StreetCode = "0500",
                    BuildingNumber = "4",
                    CitySubDivision = "New",
                    CountryCode = CountryCode.DK.Name,
                    Floor = "9",
                    Room = "9",
                    MunicipalityCode = 999,
                    IsActual = true,
                    GeoInfoReference = Guid.NewGuid(),
                };

            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);

            AssertConfirmMessage(DocumentType.ChangeMasterDataAccepted);
            var integrationEvent = FindIntegrationEvent<MasterDataChangedIntegrationEvent>();
            Assert.NotNull(integrationEvent);
            Assert.Equal(request.StreetName, integrationEvent?.StreetName);
            Assert.Equal(request.PostCode, integrationEvent?.PostCode);
            Assert.Equal(request.City, integrationEvent?.City);
            Assert.Equal(request.Floor, integrationEvent?.Floor);
            Assert.Equal(request.Room, integrationEvent?.Room);
            Assert.Equal(request.BuildingNumber, integrationEvent?.BuildingNumber);
            Assert.Equal(request.CountryCode, integrationEvent?.CountryCode);
            Assert.Equal(request.StreetCode, integrationEvent?.StreetCode);
            Assert.Equal(request.CitySubDivision, integrationEvent?.CitySubDivision);
            Assert.Equal(request.IsActual, integrationEvent?.IsActual);
            Assert.Equal(request.MunicipalityCode, integrationEvent?.MunicipalityCode);
            Assert.Equal(request.GeoInfoReference, integrationEvent?.GeoInfoReference);
        }

        [Fact]
        public async Task When_no_changes_are_made_no_integration_event_is_raised()
        {
            await CreateMeteringPointAsync().ConfigureAwait(false);
            var request = new ChangeMasterDataRequest()
                with
                {
                    TransactionId = SampleData.Transaction,
                    GsrnNumber = SampleData.GsrnNumber,
                    StreetName = SampleData.StreetName,
                };

            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);

            AssertConfirmMessage(DocumentType.ChangeMasterDataAccepted);
            AseertNoIntegrationEventIsRaised<MasterDataChangedIntegrationEvent>();
        }

        [Fact]
        public async Task Should_reject_when_street_name_is_empty()
        {
            await CreateMeteringPointAsync().ConfigureAwait(false);

            var request = new ChangeMasterDataRequest()
                with
                {
                    TransactionId = SampleData.Transaction,
                    GsrnNumber = SampleData.GsrnNumber,
                    StreetName = string.Empty,
                };

            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Should_reject_if_post_code_is_empty()
        {
            await CreateMeteringPointAsync().ConfigureAwait(false);

            var request = new ChangeMasterDataRequest()
                with
                {
                    TransactionId = SampleData.Transaction,
                    GsrnNumber = SampleData.GsrnNumber,
                    PostCode = string.Empty,
                };

            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        [Fact]
        public async Task Should_reject_if_city_is_empty()
        {
            await CreateMeteringPointAsync().ConfigureAwait(false);

            var request = new ChangeMasterDataRequest()
                with
                {
                    TransactionId = SampleData.Transaction,
                    GsrnNumber = SampleData.GsrnNumber,
                    City = string.Empty,
                };

            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);

            AssertValidationError("E86");
        }

        private Task<BusinessProcessResult> CreateMeteringPointAsync()
        {
            return InvokeBusinessProcessAsync(Scenarios.CreateConsumptionMeteringPointCommand());
        }
    }
}
