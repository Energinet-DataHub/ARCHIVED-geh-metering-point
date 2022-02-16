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
using Energinet.DataHub.MeteringPoints.Application.ChangeConnectionStatus;
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeConnectionStatus.Disconnect;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.DisconnectMeteringPoints
{
    public class DisconnectTests : TestHost
    {
        private readonly ISystemDateTimeProvider _dateTimeProvider;

        public DisconnectTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _dateTimeProvider = GetService<ISystemDateTimeProvider>();
        }

        [Fact]
        public async Task Metering_point_is_disconnected()
        {
            await CreateMeteringPointWithEnergySupplierAssigned().ConfigureAwait(false);

            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            AssertPersistedMeteringPoint
                .Initialize(SampleData.GsrnNumber, GetService<IDbConnectionFactory>())
                .HasConnectionState(PhysicalState.Connected);

            await SendCommandAsync(CreateDisconnectMeteringPointRequest()).ConfigureAwait(false);

            AssertPersistedMeteringPoint
                .Initialize(SampleData.GsrnNumber, GetService<IDbConnectionFactory>())
                .HasConnectionState(PhysicalState.Disconnected);

            AssertConfirmMessage(DocumentType.ConfirmConnectionStatusMeteringPoint);
            Assert.NotNull(FindIntegrationEvent<MeteringPointDisconnectedIntegrationEvent>());
        }

        [Fact]
        public async Task Cannot_disconnect_if_not_connected()
        {
            await CreateMeteringPointWithEnergySupplierAssigned().ConfigureAwait(false);

            await SendCommandAsync(CreateDisconnectMeteringPointRequest()).ConfigureAwait(false);

            AssertValidationError("D16");
        }

        [Theory]
        [InlineData(3, true)]
        [InlineData(1, false)]
        public async Task Effective_date_must_be_within_the_allowed_time_period_in_past(int numberOfDaysOffset, bool expectError)
        {
            var effectiveDate = EffectiveDateInPast(numberOfDaysOffset);

            await CreateMeteringPointWithEnergySupplierAssigned(effectiveDate).ConfigureAwait(false);

            await SendCommandAsync(CreateConnectMeteringPointRequest() with
            {
                EffectiveDate = effectiveDate.ToString(),
            }).ConfigureAwait(false);

            await SendCommandAsync(CreateDisconnectMeteringPointRequest() with
            {
                EffectiveDate = effectiveDate.ToString(),
            }).ConfigureAwait(false);

            AssertValidationError("E17", expectError);
        }

        [Theory]
        [InlineData(2, true)]
        [InlineData(0, false)]
        public async Task Effective_date_must_be_within_the_allowed_time_period_in_future(int numberOfDaysOffset, bool expectError)
        {
            var effectiveDate = EffectiveDateInFuture(numberOfDaysOffset);

            await CreateMeteringPointWithEnergySupplierAssigned().ConfigureAwait(false);

            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            await SendCommandAsync(CreateDisconnectMeteringPointRequest() with
            {
                EffectiveDate = effectiveDate.ToString(),
            }).ConfigureAwait(false);

            AssertValidationError("E17", expectError);
        }

        private static Instant ToEffectiveDate(DateTime date)
        {
            return TestHelpers.DaylightSavingsInstant(date);
        }

        private Task CreateMeteringPointWithEnergySupplierAssigned()
        {
            var currentDate = _dateTimeProvider.Now().ToDateTimeUtc();
            var startOfSupplyDate = TestHelpers.DaylightSavingsInstant(currentDate);
            return CreateMeteringPointWithEnergySupplierAssigned(startOfSupplyDate);
        }

        private async Task CreateMeteringPointWithEnergySupplierAssigned(Instant startOfSupplyDate)
        {
            await SendCommandAsync(Scenarios.CreateCommand(MeteringPointType.Consumption)).ConfigureAwait(false);
            await MarkAsEnergySupplierAssigned(startOfSupplyDate).ConfigureAwait(false);
        }

        private async Task MarkAsEnergySupplierAssigned(Instant startOfSupply)
        {
            var setEnergySupplierAssigned = new SetEnergySupplierInfo(SampleData.GsrnNumber, startOfSupply);
            await SendCommandAsync(setEnergySupplierAssigned).ConfigureAwait(false);
        }

        private ConnectMeteringPointRequest CreateConnectMeteringPointRequest()
        {
            var currentDate = _dateTimeProvider.Now().ToDateTimeUtc();
            var effectiveDate = TestHelpers.DaylightSavingsInstant(currentDate);
            return new(SampleData.GsrnNumber, effectiveDate.ToString(), SampleData.Transaction);
        }

        private DisconnectReconnectMeteringPointRequest CreateDisconnectMeteringPointRequest()
        {
            var currentDate = _dateTimeProvider.Now().ToDateTimeUtc();
            var effectiveDate = TestHelpers.DaylightSavingsInstant(currentDate);
            return new(SampleData.GsrnNumber, effectiveDate.ToString(), SampleData.Transaction, "Disconnected");
        }

        private Instant EffectiveDateInPast(int numberOfDaysFromToday)
        {
            return ToEffectiveDate(_dateTimeProvider.Now().Minus(Duration.FromDays(numberOfDaysFromToday)).ToDateTimeUtc());
        }

        private Instant EffectiveDateInFuture(int numberOfDaysFromToday)
        {
            return ToEffectiveDate(_dateTimeProvider.Now().Plus(Duration.FromDays(numberOfDaysFromToday)).ToDateTimeUtc());
        }
    }
}
