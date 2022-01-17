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

using System;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.ChangeMasterData.Consumption;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.UpdateMasterData.ConsumptionMeteringPoints
{
    [IntegrationTest]
    public class ChangeTests : TestHost
    {
        private readonly ISystemDateTimeProvider _timeProvider;

        public ChangeTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _timeProvider = GetService<ISystemDateTimeProvider>();
        }

        [Fact]
        public async Task Connection_type_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction() with
            {
                ConnectionType = ConnectionType.Installation.Name,
            }).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ConnectionType = ConnectionType.Direct.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasConnectionType(ConnectionType.Direct);
        }

        [Fact]
        public async Task Scheduled_meter_reading_date_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand() with
            {
                ScheduledMeterReadingDate = "0101",
            }).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ScheduledMeterReadingDate = "0201",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasScheduledMeterReadingDate("0201");
        }

        [Fact]
        public async Task Settlement_method_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand() with
            {
                SettlementMethod = SettlementMethod.Flex.Name,
            }).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    SettlementMethod = SettlementMethod.NonProfiled.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasSettlementMethod(SettlementMethod.NonProfiled);
        }

        [Fact]
        public async Task Metering_configuration_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeteringMethod = MeteringMethod.Physical.Name,
                    MeterId = "123",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasMeteringConfiguration(MeteringMethod.Physical, "123");
        }

        [Fact]
        public async Task Capacity_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    CapacityInKw = "1",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasCapacity(1);
        }

        [Fact]
        public async Task Effective_date_is_stored()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    PowerPlantGsrnNumber = SampleData.PowerPlantGsrnNumber,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasEffectiveDate(EffectiveDate.Create(request.EffectiveDate));
        }

        [Fact]
        public async Task Power_plant_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    PowerPlantGsrnNumber = SampleData.PowerPlantGsrnNumber,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasPowerPlantGsrnNumber(SampleData.PowerPlantGsrnNumber);
        }

        [Fact]
        public async Task Power_limit_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction() with
            {
                MaximumCurrent = 1,
                MaximumPower = 1,
            }).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MaximumCurrent = "2",
                    MaximumPower = string.Empty,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasPowerLimit(null, 2);
        }

        [Fact]
        public async Task Reading_occurrence_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ReadingPeriodicity = ReadingOccurrence.Monthly.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasReadingOccurrence(ReadingOccurrence.Monthly);
        }

        [Fact]
        public async Task Asset_type_is_changed()
        {
            await InvokeBusinessProcessAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    AssetType = AssetType.Boiler.Name,
                };

            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasAssetType(AssetType.Boiler);
        }

        [Fact]
        public async Task Unit_type_is_changed()
        {
            await InvokeBusinessProcessAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    UnitType = MeasurementUnitType.Ampere.Name,
                };

            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasUnitType(MeasurementUnitType.Ampere);
        }

        [Fact]
        public async Task Product_type_is_changed()
        {
            await InvokeBusinessProcessAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ProductType = ProductType.Tariff.Name,
                };
            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasProductType(ProductType.Tariff);
        }

        [Fact]
        public async Task Address_is_updated()
        {
            await CreatePhysicalConsumptionMeteringPoint().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    Address = new Application.Address(
                        StreetName: "New Street Name",
                        PostCode: "6000",
                        City: "New City Name",
                        StreetCode: "0500",
                        BuildingNumber: "4",
                        CitySubDivision: "New",
                        CountryCode: CountryCode.DK.Name,
                        Floor: "9",
                        Room: "9",
                        MunicipalityCode: 999,
                        IsActual: true,
                        GeoInfoReference: Guid.NewGuid()),
                };

            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);

            AssertConfirmMessage(DocumentType.ConfirmChangeMasterData);
            AssertMasterData()
                .HasStreetName(request.Address.StreetName)
                .HasPostCode(request.Address.PostCode)
                .HasCity(request.Address.City)
                .HasStreetCode(request.Address.StreetCode)
                .HasBuildingNumber(request.Address.BuildingNumber)
                .HasCitySubDivision(request.Address.CitySubDivision)
                .HasCountryCode(CountryCode.DK)
                .HasFloor(request.Address.Floor)
                .HasRoom(request.Address.Room)
                .HasMunicipalityCode(request.Address.MunicipalityCode)
                .HasIsActualAddress(request.Address.IsActual)
                .HasGeoInfoReference(request.Address.GeoInfoReference);
        }

        [Fact]
        public async Task Transaction_id_is_required()
        {
            await InvokeBusinessProcessAsync(TestUtils.CreateRequest()
                with
                {
                    TransactionId = string.Empty,
                }).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Metering_point_must_exist()
        {
            await InvokeBusinessProcessAsync(TestUtils.CreateRequest()).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Theory]
        [InlineData("invalid_gsrn_number")]
        [InlineData("")]
        public async Task Gsrn_number_is_required(string gsrnNumber)
        {
            await InvokeBusinessProcessAsync(TestUtils.CreateRequest()
                with
                {
                    GsrnNumber = gsrnNumber,
                }).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Effective_date_is_required()
        {
            await InvokeBusinessProcessAsync(TestUtils.CreateRequest()
                with
                {
                    EffectiveDate = string.Empty,
                }).ConfigureAwait(false);

            AssertValidationError("D02");
        }

        [Theory]
        [InlineData("2021-01-01T18:00:00Z", "2021-01-02T22:00:00Z", true)]
        [InlineData("2021-01-01T18:00:00Z", "2020-12-30T22:00:00Z", true)]
        [InlineData("2021-01-01T18:00:00Z", "2021-01-01T22:00:00Z", false)]
        [InlineData("2021-01-01T18:00:00Z", "2020-12-31T22:00:00Z", false)]
        public async Task Effective_date_is_today_or_the_day_before(string today, string effectiveDate, bool expectError)
        {
            var timeProvider = GetService<ISystemDateTimeProvider>() as SystemDateTimeProviderStub;
            timeProvider!.SetNow(InstantPattern.General.Parse(today).Value);

            await CreateMeteringPointAsync().ConfigureAwait(false);

            await InvokeBusinessProcessAsync(TestUtils.CreateRequest()
                with
                {
                    EffectiveDate = effectiveDate,
                }).ConfigureAwait(false);

            AssertValidationError("E17", expectError);
        }

        [Fact]
        public async Task Grid_operator_is_the_owner_of_the_metering_point()
        {
            await CreateMeteringPointAsync().ConfigureAwait(false);

            SetGridOperatorAsAuthenticatedUser("820000000140x"); // This is not the owner of this metering point
            await InvokeBusinessProcessAsync(TestUtils.CreateRequest()).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Meter_is_required_when_physical()
        {
            var timeProvider = GetService<ISystemDateTimeProvider>() as SystemDateTimeProviderStub;
            timeProvider!.SetNow(InstantPattern.General.Parse(SampleData.EffectiveDate).Value);

            await CreatePhysicalConsumptionMeteringPoint().ConfigureAwait(false);

            var request = TestUtils.CreateRequest()
                with
                {
                    MeterId = string.Empty,
                };
            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);

            AssertValidationError("D31");
        }

        [Fact]
        public async Task Can_not_change_when_metering_point_is_closed_down()
        {
            await CreatePhysicalConsumptionMeteringPoint().ConfigureAwait(false);
            await CloseDownMeteringPointAsync().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeterId = "1",
                };
            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);

            AssertValidationError("D16");
        }

        private Task<BusinessProcessResult> CreateMeteringPointAsync()
        {
            return InvokeBusinessProcessAsync(Scenarios.CreateConsumptionMeteringPointCommand());
        }

        private async Task CreatePhysicalConsumptionMeteringPoint()
        {
            var request = Scenarios.CreateConsumptionMeteringPointCommand()
                with
                {
                    MeteringMethod = MeteringMethod.Physical.Name,
                    MeterNumber = "1",
                    NetSettlementGroup = NetSettlementGroup.Zero.Name,
                    ConnectionType = null,
                    ScheduledMeterReadingDate = null,
                };
            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);
        }

        private async Task CreateConsumptionMeteringPointInNetSettlementGroup6()
        {
            var request = Scenarios.CreateConsumptionMeteringPointCommand()
                with
                {
                    EffectiveDate = CreateEffectiveDateAsOfToday().ToString(),
                    MeteringMethod = MeteringMethod.Virtual.Name,
                    NetSettlementGroup = NetSettlementGroup.Six.Name,
                    ConnectionType = ConnectionType.Installation.Name,
                    ScheduledMeterReadingDate = "0101",
                };
            await InvokeBusinessProcessAsync(request).ConfigureAwait(false);
        }

        private EffectiveDate CreateEffectiveDateAsOfToday()
        {
            var today = _timeProvider.Now().ToDateTimeUtc();
            return EffectiveDate.Create(new DateTime(today.Year, today.Month, today.Day, 22, 0, 0));
        }

        private ChangeMasterDataRequest CreateUpdateRequest()
        {
            return TestUtils.CreateRequest()
                with
                {
                    TransactionId = SampleData.Transaction,
                    GsrnNumber = SampleData.GsrnNumber,
                    EffectiveDate = CreateEffectiveDateAsOfToday().ToString(),
                };
        }

        private AssertPersistedMeteringPoint AssertMasterData()
        {
            return AssertPersistedMeteringPoint
                .Initialize(SampleData.GsrnNumber, GetService<IDbConnectionFactory>());
        }
    }
}