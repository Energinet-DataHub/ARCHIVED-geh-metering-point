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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.Consumption
{
    [UnitTest]
    public class CreationTests : TestBase
    {
        [Fact]
        public void Should_succeed()
        {
            var meteringPointId = MeteringPointId.New();
            var meteringPointGsrn = GsrnNumber.Create(SampleData.GsrnNumber);
            var isOfficielAddress = SampleData.IsOfficialAddress;
            var meteringPointSubtype = MeteringPointSubType.Physical;
            var gridAreadLinkId = new GridAreaLinkId(Guid.Parse(SampleData.GridAreaLinkId));
            var powerPlanGsrn = GsrnNumber.Create(SampleData.PowerPlant);
            var netSettlementGroup = NetSettlementGroup.Three;
            var locationDescription = LocationDescription.Create(string.Empty);
            var measurementUnitType = MeasurementUnitType.KWh;
            var meterNumber = MeterId.Create("A1234");
            var readingOccurrence = ReadingOccurrence.Hourly;
            var powerLimit = PowerLimit.Create(0, 0);
            var effectiveDate = EffectiveDate.Create(SampleData.EffectiveDate);
            var settlementMethod = SettlementMethod.Flex;
            var disconnectionType = DisconnectionType.Manual;
            var connectionType = ConnectionType.Direct;
            var assetType = AssetType.GasTurbine;
            var address = Address.Create(
                streetName: "Test Street",
                streetCode: "1000",
                buildingNumber: string.Empty,
                city: "Test City",
                citySubDivision: string.Empty,
                postCode: "8000",
                countryCode: CountryCode.DK,
                floor: string.Empty,
                room: string.Empty,
                municipalityCode: null,
                isOfficial: true,
                geoInfoReference: Guid.NewGuid());
            var scheduledMeterReadingDate = ScheduledMeterReadingDate.Create("0101");

            var meteringPointDetails = CreateDetails()
                with
                {
                    Id = meteringPointId,
                    Address = address,
                    GridAreaLinkId = gridAreadLinkId,
                    LocationDescription = locationDescription,
                    MeterNumber = meterNumber,
                    PowerLimit = powerLimit,
                    DisconnectionType = disconnectionType,
                    ConnectionType = connectionType,
                    ScheduledMeterReadingDate = scheduledMeterReadingDate,
                };

            var meteringPoint = ConsumptionMeteringPoint.Create(meteringPointDetails);

            var createdEvent = meteringPoint.DomainEvents.First(e => e is ConsumptionMeteringPointCreated) as ConsumptionMeteringPointCreated;
            Assert.Equal(address.City, createdEvent!.City);
            Assert.Equal(address.Floor, createdEvent.Floor);
            Assert.Equal(address.Room, createdEvent.Room);
            Assert.Equal(address.BuildingNumber, createdEvent.BuildingNumber);
            Assert.Equal(address.CountryCode?.Name, createdEvent.CountryCode);
            Assert.Equal(address.MunicipalityCode, createdEvent.MunicipalityCode);
            Assert.Equal(address.PostCode, createdEvent.PostCode);
            Assert.Equal(address.StreetCode, createdEvent.StreetCode);
            Assert.Equal(address.StreetName, createdEvent.StreetName);
            Assert.Equal(address.CitySubDivision, createdEvent.CitySubDivision);
            Assert.Equal(address.IsOfficial, createdEvent.IsOfficialAddress);
            Assert.Equal(address.GeoInfoReference, createdEvent.GeoInfoReference);
            Assert.Equal(meteringPointId.Value, createdEvent.MeteringPointId);
            Assert.Equal(meteringPointGsrn.Value, createdEvent.GsrnNumber);
            Assert.Equal(isOfficielAddress, createdEvent.IsOfficialAddress);
            Assert.Equal(meteringPointSubtype.Name, createdEvent.MeteringPointSubType);
            Assert.Equal(gridAreadLinkId.Value, createdEvent.GridAreaLinkId);
            Assert.Equal(meteringPointDetails.NetSettlementGroup.Name, createdEvent.NetSettlementGroup);
            Assert.Equal(powerPlanGsrn.Value, createdEvent.PowerPlantGsrnNumber);
            Assert.Equal(locationDescription.Value, createdEvent.LocationDescription);
            Assert.Equal(measurementUnitType.Name, createdEvent.UnitType);
            Assert.Equal(meterNumber.Value, createdEvent.MeterNumber);
            Assert.Equal(readingOccurrence.Name, createdEvent.ReadingOccurrence);
            Assert.Equal(powerLimit.Ampere, createdEvent.MaximumCurrent);
            Assert.Equal(powerLimit.Kwh, createdEvent.MaximumPower);
            Assert.Equal(effectiveDate.DateInUtc, createdEvent.EffectiveDate);
            Assert.Equal(settlementMethod.Name, createdEvent.SettlementMethod);
            Assert.Equal(disconnectionType.Name, createdEvent.DisconnectionType);
            Assert.Equal(connectionType.Name, createdEvent.ConnectionType);
            Assert.Equal(assetType.Name, createdEvent.AssetType);
            Assert.Equal(scheduledMeterReadingDate.MonthAndDay, createdEvent.ScheduledMeterReadingDate);
        }

        [Fact]
        public void Day_of_scheduled_meter_reading_date_must_be_01_when_net_settlement_group_is_6()
        {
            var details = CreateDetails()
                with
                {
                    ScheduledMeterReadingDate = ScheduledMeterReadingDate.Create("0512"),
                    NetSettlementGroup = NetSettlementGroup.Six,
                };

            var checkResult = ConsumptionMeteringPoint.CanCreate(details);

            AssertContainsValidationError<InvalidScheduledMeterReadingDateNetSettlementGroupRuleError>(checkResult);
        }

        [Fact]
        public void Scheduled_meter_reading_date_is_not_allowed_for_other_than_net_settlement_group_6()
        {
            var details = CreateDetails()
                with
                {
                    ScheduledMeterReadingDate = ScheduledMeterReadingDate.Create("0512"),
                    NetSettlementGroup = NetSettlementGroup.One,
                };

            var checkResult = ConsumptionMeteringPoint.CanCreate(details);

            AssertContainsValidationError<ScheduledMeterReadingDateNotAllowedRuleError>(checkResult);
        }

        [Fact]
        public void Product_type_should_as_default_be_active_energy()
        {
            var details = CreateDetails();

            var meteringPoint = ConsumptionMeteringPoint.Create(details);

            var createdEvent = meteringPoint.DomainEvents.First(e => e is ConsumptionMeteringPointCreated) as ConsumptionMeteringPointCreated;
            Assert.Equal(ProductType.EnergyActive.Name, createdEvent!.ProductType);
        }

        [Theory]
        [InlineData(nameof(NetSettlementGroup.Six))]
        public void Powerplant_GSRN_is_required_when_netsettlementgroup_is_other_than_0_or_99(string netSettlementGroupName)
        {
            var netSettlementGroup = NetSettlementGroup.FromName<NetSettlementGroup>(netSettlementGroupName);
            var meteringPointDetails = CreateDetails()
                with
                {
                    PowerPlantGsrnNumber = null,
                    NetSettlementGroup = netSettlementGroup,
                };

            var checkResult = CreateRequest(meteringPointDetails);

            AssertContainsValidationError<PowerPlantIsRequiredForNetSettlementGroupRuleError>(checkResult);
        }

        [Theory]
        [InlineData(nameof(NetSettlementGroup.Zero))]
        [InlineData(nameof(NetSettlementGroup.Ninetynine))]
        public void Powerplant_GSRN_is_not_required_when_netsettlementgroup_is_0_or_99(string netSettlementGroupName)
        {
            var netSettlementGroup = NetSettlementGroup.FromName<NetSettlementGroup>(netSettlementGroupName);
            var meteringPointDetails = CreateDetails()
                with
                {
                    NetSettlementGroup = netSettlementGroup,
                };

            var checkResult = CreateRequest(meteringPointDetails);

            AssertDoesNotContainValidationError<PowerPlantIsRequiredForNetSettlementGroupRuleError>(checkResult);
        }

        [Fact]
        public void Should_return_error_when_street_name_is_missing()
        {
            var address = Address.Create(
                string.Empty,
                SampleData.StreetCode,
                string.Empty,
                SampleData.CityName,
                string.Empty,
                string.Empty,
                null,
                string.Empty,
                string.Empty,
                default,
                isOfficial: true,
                geoInfoReference: Guid.NewGuid());

            var meteringPointDetails = CreateDetails()
                with
                {
                    Address = address,
                };

            var checkResult = CreateRequest(meteringPointDetails);
            AssertContainsValidationError<StreetNameIsRequiredRuleError>(checkResult);
        }

        [Fact]
        public void Should_return_error_when_post_code_is_missing()
        {
            var address = Address.Create(
                SampleData.StreetName,
                SampleData.StreetCode,
                string.Empty,
                SampleData.CityName,
                string.Empty,
                string.Empty,
                null,
                string.Empty,
                string.Empty,
                default,
                isOfficial: true,
                geoInfoReference: Guid.NewGuid());

            var meteringPointDetails = CreateDetails()
                with
                {
                    Address = address,
                };

            var checkResult = ConsumptionMeteringPoint.CanCreate(meteringPointDetails);

            Assert.Contains(checkResult.Errors, error => error is PostCodeIsRequiredRuleError);
        }

        [Fact]
        public void Product_type_should_be_set_to_active_energy()
        {
            var address = CreateAddress();

            var meteringPointDetails = CreateDetails()
                with
                {
                    Address = address,
                };

            var meteringPoint = ConsumptionMeteringPoint.Create(meteringPointDetails);

            var createdEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is ConsumptionMeteringPointCreated) as ConsumptionMeteringPointCreated;

            Assert.Equal(ProductType.EnergyActive.Name, createdEvent!.ProductType);
        }

        private static BusinessRulesValidationResult CreateRequest(MeteringPointDetails meteringPointDetails)
        {
            return ConsumptionMeteringPoint.CanCreate(meteringPointDetails);
        }

        private static void AssertContainsValidationError<TValidationError>(BusinessRulesValidationResult result)
        {
            Assert.Contains(result.Errors, error => error is TValidationError);
        }

        private static void AssertDoesNotContainValidationError<TValidationError>(BusinessRulesValidationResult result)
        {
            Assert.DoesNotContain(result.Errors, error => error is TValidationError);
        }
    }
}
