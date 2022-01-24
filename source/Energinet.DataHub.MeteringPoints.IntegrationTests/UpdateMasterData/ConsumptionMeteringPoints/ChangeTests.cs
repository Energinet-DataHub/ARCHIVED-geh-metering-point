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
using System.Globalization;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
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
                    MeterNumber = "123",
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasMeteringConfiguration(MeteringMethod.Physical, "123");
        }

        [Fact]
        public async Task Power_plant_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    PowerPlant = SampleData.PowerPlantGsrnNumber,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasPowerPlantGsrnNumber(SampleData.PowerPlantGsrnNumber);
        }

        [Fact]
        public async Task Reading_occurrence_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeterReadingOccurrence = ReadingOccurrence.Monthly.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasReadingOccurrence(ReadingOccurrence.Monthly);
        }

        [Fact]
        public async Task Unit_type_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeasureUnitType = MeasurementUnitType.Ampere.Name,
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasUnitType(MeasurementUnitType.Ampere);
        }

        [Fact]
        public async Task Product_type_is_changed()
        {
            await SendCommandAsync(Scenarios.CreateVEProduction()).ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    ProductType = ProductType.Tariff.Name,
                };
            await SendCommandAsync(request).ConfigureAwait(false);

            AssertMasterData()
                .HasProductType(ProductType.Tariff);
        }

        [Fact]
        public async Task Address_is_updated()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    StreetName = "New Street Name",
                        PostCode = "6000",
                        CityName = "New City Name",
                        StreetCode = "0500",
                        BuildingNumber = "4",
                        CitySubDivisionName = "New",
                        CountryCode = CountryCode.DK.Name,
                        FloorIdentification = "9",
                        RoomIdentification = "9",
                        MunicipalityCode = "999",
                        IsActualAddress = true,
                        GeoInfoReference = Guid.NewGuid().ToString(),
                };

            await SendCommandAsync(request).ConfigureAwait(false);

            AssertConfirmMessage(DocumentType.ConfirmChangeMasterData);
            AssertMasterData()
                .HasStreetName(request.StreetName)
                .HasPostCode(request.PostCode)
                .HasCity(request.CityName)
                .HasStreetCode(request.StreetCode)
                .HasBuildingNumber(request.BuildingNumber)
                .HasCitySubDivision(request.CitySubDivisionName)
                .HasCountryCode(CountryCode.DK)
                .HasFloor(request.FloorIdentification)
                .HasRoom(request.RoomIdentification)
                .HasMunicipalityCode(int.Parse(request.MunicipalityCode, CultureInfo.InvariantCulture))
                .HasIsActualAddress(request.IsActualAddress)
                .HasGeoInfoReference(Guid.Parse(request.GeoInfoReference));
        }

        [Fact]
        public async Task Transaction_id_is_required()
        {
            await SendCommandAsync(TestUtils.CreateRequest()
                with
                {
                    TransactionId = string.Empty,
                }).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Metering_point_must_exist()
        {
            await SendCommandAsync(TestUtils.CreateRequest()).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Theory]
        [InlineData("invalid_gsrn_number")]
        [InlineData("")]
        public async Task Gsrn_number_is_required(string gsrnNumber)
        {
            await SendCommandAsync(TestUtils.CreateRequest()
                with
                {
                    GsrnNumber = gsrnNumber,
                }).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Effective_date_is_required()
        {
            await SendCommandAsync(TestUtils.CreateRequest()
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

            await SendCommandAsync(TestUtils.CreateRequest()
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
            await SendCommandAsync(TestUtils.CreateRequest()).ConfigureAwait(false);

            AssertValidationError("E10");
        }

        [Fact]
        public async Task Meter_is_required_when_physical()
        {
            var timeProvider = GetService<ISystemDateTimeProvider>() as SystemDateTimeProviderStub;
            timeProvider!.SetNow(InstantPattern.General.Parse(SampleData.EffectiveDate).Value);

            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);

            var request = TestUtils.CreateRequest()
                with
                {
                    MeterNumber = string.Empty,
                };
            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D31");
        }

        [Fact]
        public async Task Can_not_change_when_metering_point_is_closed_down()
        {
            await CreatePhysicalConsumptionMeteringPointAsync().ConfigureAwait(false);
            await CloseDownMeteringPointAsync().ConfigureAwait(false);

            var request = CreateUpdateRequest()
                with
                {
                    MeterNumber = "1",
                };
            await SendCommandAsync(request).ConfigureAwait(false);

            AssertValidationError("D16");
        }

        private Task CreateMeteringPointAsync()
        {
            return SendCommandAsync(Scenarios.CreateConsumptionMeteringPointCommand());
        }
    }
}
