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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Application.Extensions;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.Connect;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.ConnectMeteringPoints
{
    [IntegrationTest]
    public class ConnectTests
        : TestHost
    {
        private readonly ISystemDateTimeProvider _dateTimeProvider;

        public ConnectTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _dateTimeProvider = GetService<ISystemDateTimeProvider>();
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.NetProduction))]
        [InlineData(nameof(MeteringPointType.SupplyToGrid))]
        [InlineData(nameof(MeteringPointType.ConsumptionFromGrid))]
        public async Task Metering_point_must_be_physical(string meteringPointType)
        {
            await SendCommandAsync(
                    Scenarios.CreateCommand(EnumerationType.FromName<MeteringPointType>(meteringPointType))
                        with
                        {
                            MeteringMethod = MeteringMethod.Calculated.Name, MeterNumber = null,
                        })
                .ConfigureAwait(false);

            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            AssertValidationError("D37");
        }

        [Fact]
        public async Task Must_be_coupled_to_parent_to_be_connected()
        {
            await SendCommandAsync(Scenarios.CreateExchangeReactiveEnergy()
                with
                {
                    MeteringMethod = MeteringMethod.Physical.Name,
                    MeterNumber = "Fake",
                }).ConfigureAwait(false);

            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            AssertValidationError("D36");
        }

        [Theory]
        [InlineData(8, true, true)]
        [InlineData(5, true, false)]
        [InlineData(8, false, true)]
        public async Task Effective_date_must_be_within_the_allowed_time_period(int numberOfDaysOffset, bool effectiveDateIsInThePast, bool expectError)
        {
            var effectiveDate = effectiveDateIsInThePast
                ? EffectiveDateInPast(numberOfDaysOffset)
                : EffectiveDateInFuture(numberOfDaysOffset);

            await CreateMeteringPointWithEnergySupplierAssigned(effectiveDate).ConfigureAwait(false);

            await SendCommandAsync(CreateConnectMeteringPointRequest() with
            {
                EffectiveDate = effectiveDate.ToString(),
            }).ConfigureAwait(false);

            AssertValidationError("E17", expectError);
        }

        [Fact]
        public async Task Requesting_user_must_be_the_owner_of_the_metering_point()
        {
            await CreateMeteringPointWithEnergySupplierAssigned().ConfigureAwait(false);

            SetCurrentAuthenticatedActor(Guid.NewGuid());
            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Cannot_connect_if_already_connected()
        {
            await CreateMeteringPointWithEnergySupplierAssigned().ConfigureAwait(false);
            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            AssertValidationError("D16");
        }

        [Fact]
        public async Task Cannot_connect_if_closed_down()
        {
            await CreateMeteringPointWithEnergySupplierAssigned().ConfigureAwait(false);
            await CloseDownMeteringPointAsync().ConfigureAwait(false);

            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            AssertValidationError("D16");
        }

        [Fact]
        public async Task Metering_point_must_exist()
        {
            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Metering_point_is_connected()
        {
            await CreateMeteringPointWithEnergySupplierAssigned().ConfigureAwait(false);

            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            AssertPersistedMeteringPoint
                .Initialize(SampleData.GsrnNumber, GetService<IDbConnectionFactory>())
                .HasConnectionState(PhysicalState.Connected);
            AssertConfirmMessage(DocumentType.ConfirmConnectMeteringPoint, "D15");
            Assert.NotNull(FindIntegrationEvent<MeteringPointConnectedIntegrationEvent>());
        }

        [Fact]
        public async Task Reject_if_business_rules_are_violated()
        {
            await CreateMeteringPointWithoutEnergySupplierAssigned(EnumerationType.FromName<MeteringPointType>(nameof(MeteringPointType.Consumption))).ConfigureAwait(false);
            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            AssertRejectMessage(DocumentType.RejectConnectMeteringPoint, "D15");
        }

        [Fact]
        public async Task Confirm_should_contain_correct_business_reason_code()
        {
            await CreateMeteringPointWithEnergySupplierAssigned().ConfigureAwait(false);
            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            AssertRejectMessage(DocumentType.ConfirmConnectMeteringPoint, "D15");
        }

        [Fact]
        public async Task Metering_point_connected_shows_in_process_overview()
        {
            await CreateMeteringPointWithEnergySupplierAssigned().ConfigureAwait(false);

            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            await AssertProcessOverviewAsync(
                    SampleData.GsrnNumber,
                    "BRS-008",
                    "RequestConnectMeteringPoint",
                    "ConfirmConnectMeteringPoint")
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task Send_master_data_to_associated_energy_suppliers_when_connected()
        {
            var createMeteringPointRequest = CreateMeteringPointRequest();
            var connectMeteringPointRequest = CreateConnectMeteringPointRequest();

            await SendCommandAsync(createMeteringPointRequest, CancellationToken.None).ConfigureAwait(false);
            await MarkAsEnergySupplierAssigned(connectMeteringPointRequest.EffectiveDate.ToInstant()).ConfigureAwait(false);
            await AddEnergySupplier(connectMeteringPointRequest.EffectiveDate.ToInstant()).ConfigureAwait(false);
            await AddEnergySupplier(connectMeteringPointRequest.EffectiveDate.ToInstant().Plus(Duration.FromDays(2))).ConfigureAwait(false);
            await SendCommandAsync(connectMeteringPointRequest, CancellationToken.None).ConfigureAwait(false);

            await AssertAndRunInternalCommandAsync<SendAccountingPointCharacteristicsMessage>().ConfigureAwait(false);

            AssertOutboxMessage<MessageHubEnvelope>(envelope => envelope.MessageType == DocumentType.AccountingPointCharacteristicsMessage, 2);
        }

        [Theory]
        [InlineData(nameof(MeteringPointType.Consumption))]
        [InlineData(nameof(MeteringPointType.Production))]
        public async Task Cannot_connect_metering_point_when_no_energy_supplier_is_assigned(string meteringPointType)
        {
            await CreateMeteringPointWithoutEnergySupplierAssigned(EnumerationType.FromName<MeteringPointType>(meteringPointType)).ConfigureAwait(false);

            await SendCommandAsync(CreateConnectMeteringPointRequest()).ConfigureAwait(false);

            AssertValidationError("D36");
        }

        [Fact]
        public async Task Effective_date_is_required()
        {
            var createMeteringPointRequest = CreateMeteringPointRequest();
            var connectMeteringPointRequest = CreateConnectMeteringPointRequest()
                with
                {
                    EffectiveDate = string.Empty,
                };

            await SendCommandAsync(createMeteringPointRequest, CancellationToken.None).ConfigureAwait(false);
            await SendCommandAsync(connectMeteringPointRequest, CancellationToken.None).ConfigureAwait(false);

            AssertOutboxMessage<MessageHubEnvelope>(envelope => envelope.MessageType == DocumentType.RejectConnectMeteringPoint);
        }

        private static MasterDataDocument CreateMeteringPointRequest()
        {
            return new(
                BusinessProcessType.CreateMeteringPoint.Name,
                SampleData.StreetName,
                SampleData.BuildingNumber,
                SampleData.PostCode,
                SampleData.CityName,
                SampleData.CitySubDivisionName,
                SampleData.MunicipalityCode,
                SampleData.CountryCode,
                SampleData.StreetCode,
                SampleData.FloorIdentification,
                SampleData.RoomIdentification,
                SampleData.IsActualAddress,
                SampleData.GsrnNumber,
                SampleData.TypeOfMeteringPoint,
                SampleData.SubTypeOfMeteringPoint,
                SampleData.ReadingOccurrence,
                "0",
                "0",
                SampleData.MeteringGridArea,
                SampleData.PowerPlantGsrnNumber,
                string.Empty,
                SampleData.SettlementMethod,
                SampleData.DisconnectionType,
                SampleData.EffectiveDate,
                SampleData.MeterNumber,
                SampleData.Transaction,
                SampleData.PhysicalState,
                SampleData.NetSettlementGroup,
                SampleData.ConnectionType,
                SampleData.AssetType,
                null,
                ToGrid: null,
                ParentRelatedMeteringPoint: null,
                SampleData.ProductType,
                null,
                SampleData.GeoInfoReference,
                SampleData.MeasurementUnitType,
                SampleData.ScheduledMeterReadingDate);
        }

        private static Instant ToEffectiveDate(DateTime date)
        {
            return TestHelpers.DaylightSavingsInstant(date);
        }

        private ConnectMeteringPointRequest CreateConnectMeteringPointRequest()
        {
            var effectiveDate = CreateEffectiveDateAsOfToday();
            return new(SampleData.GsrnNumber, effectiveDate.ToString(), SampleData.Transaction);
        }

        private async Task MarkAsEnergySupplierAssigned(Instant startOfSupply)
        {
            var setEnergySupplierAssigned = new SetEnergySupplierInfo(SampleData.GsrnNumber, startOfSupply);
            await SendCommandAsync(setEnergySupplierAssigned).ConfigureAwait(false);
        }

        private async Task AddEnergySupplier(Instant startOfSupply)
        {
            var meteringPointRepository = GetService<IMeteringPointRepository>();
            var meteringPoint = await meteringPointRepository.GetByGsrnNumberAsync(GsrnNumber.Create(SampleData.GsrnNumber))
                .ConfigureAwait(false);

            var addEnergySupplier = new AddEnergySupplier(meteringPoint?.Id.Value.ToString()!, startOfSupply, SampleData.GlnNumber);
            await SendCommandAsync(addEnergySupplier).ConfigureAwait(false);
        }

        private Task CreateMeteringPointWithEnergySupplierAssigned()
        {
            var currentDate = _dateTimeProvider.Now().ToDateTimeUtc();
            var startOfSupplyDate = Instant.FromUtc(currentDate.Year, currentDate.Month, currentDate.Day, 22, 0);
            return CreateMeteringPointWithEnergySupplierAssigned(startOfSupplyDate);
        }

        private async Task CreateMeteringPointWithEnergySupplierAssigned(Instant startOfSupplyDate)
        {
            await SendCommandAsync(Scenarios.CreateCommand(MeteringPointType.Consumption)).ConfigureAwait(false);
            await MarkAsEnergySupplierAssigned(startOfSupplyDate).ConfigureAwait(false);
        }

        private async Task CreateMeteringPointWithoutEnergySupplierAssigned(MeteringPointType meteringPointType)
        {
            await SendCommandAsync(Scenarios.CreateCommand(meteringPointType)).ConfigureAwait(false);
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
