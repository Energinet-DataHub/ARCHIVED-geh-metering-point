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
    public class CreationTests
    {
        [Fact]
        public void Should_succeed()
        {
            var meteringPointId = MeteringPointId.New();
            var meteringPointGsrn = GsrnNumber.Create(SampleData.GsrnNumber);
            var isOfficielAddress = SampleData.IsOfficialAddress;
            var meteringPointSubtype = MeteringPointSubType.Physical;
            var gridAreadId = GridAreaId.New();
            var powerPlanGsrn = GsrnNumber.Create(SampleData.PowerPlant);
            var netSettlementGroup = NetSettlementGroup.Three;
            var locationDescription = "Test";
            var measurementUnitType = MeasurementUnitType.KWh;
            var meterNumber = "1";
            var readingOccurrence = ReadingOccurrence.Hourly;
            var maximumCurrent = 1;
            var maximumPower = 2;
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
                countryCode: string.Empty,
                floor: string.Empty,
                room: string.Empty,
                municipalityCode: null);
            var scheduledMeterReadingDate = ScheduledMeterReadingDate.Create("0101");

            var meteringPointDetails = CreateDetails()
                with
                {
                    Id = meteringPointId,
                    Address = address,
                    GridAreaId = gridAreadId,
                    LocationDescription = locationDescription,
                    MeterNumber = meterNumber,
                    MaximumCurrent = maximumCurrent,
                    MaximumPower = maximumPower,
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
            Assert.Equal(address.CountryCode, createdEvent.CountryCode);
            Assert.Equal(address.MunicipalityCode, createdEvent.MunicipalityCode);
            Assert.Equal(address.PostCode, createdEvent.PostCode);
            Assert.Equal(address.StreetCode, createdEvent.StreetCode);
            Assert.Equal(address.StreetName, createdEvent.StreetName);
            Assert.Equal(address.CitySubDivision, createdEvent.CitySubDivision);
            Assert.Equal(meteringPointId.Value, createdEvent.MeteringPointId);
            Assert.Equal(meteringPointGsrn.Value, createdEvent.GsrnNumber);
            Assert.Equal(isOfficielAddress, createdEvent.IsOfficielAddress);
            Assert.Equal(meteringPointSubtype.Name, createdEvent.MeteringPointSubType);
            Assert.Equal(gridAreadId.Value, createdEvent.GridAreaId);
            Assert.Equal(netSettlementGroup.Name, createdEvent.NetSettlementGroup);
            Assert.Equal(powerPlanGsrn.Value, createdEvent.PowerPlantGsrnNumber);
            Assert.Equal(locationDescription, createdEvent.LocationDescription);
            Assert.Equal(measurementUnitType.Name, createdEvent.UnitType);
            Assert.Equal(meterNumber, createdEvent.MeterNumber);
            Assert.Equal(readingOccurrence.Name, createdEvent.ReadingOccurrence);
            Assert.Equal(maximumCurrent, createdEvent.MaximumCurrent);
            Assert.Equal(maximumPower, createdEvent.MaximumPower);
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
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: string.Empty,
                city: string.Empty,
                citySubDivision: string.Empty,
                postCode: string.Empty,
                countryCode: string.Empty,
                floor: string.Empty,
                room: string.Empty,
                municipalityCode: default);

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
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: string.Empty,
                city: string.Empty,
                citySubDivision: string.Empty,
                postCode: string.Empty,
                countryCode: string.Empty,
                floor: string.Empty,
                room: string.Empty,
                municipalityCode: default);

            var meteringPointDetails = CreateDetails()
                with
                {
                    Address = address,
                };

            var checkResult = ConsumptionMeteringPoint.CanCreate(meteringPointDetails);

            Assert.Contains(checkResult.Errors, error => error is PostCodeIsRequiredRuleError);
        }

        [Fact]
        public void Should_return_error_when_city_is_missing()
        {
            var address = Address.Create(
                streetName: string.Empty,
                streetCode: string.Empty,
                buildingNumber: string.Empty,
                city: string.Empty,
                citySubDivision: string.Empty,
                postCode: string.Empty,
                countryCode: string.Empty,
                floor: string.Empty,
                room: string.Empty,
                municipalityCode: default);

            var meteringPointDetails = CreateDetails()
                with
                {
                    Address = address,
                };

            var checkResult = ConsumptionMeteringPoint.CanCreate(meteringPointDetails);

            Assert.Contains(checkResult.Errors, error => error is CityIsRequiredRuleError);
        }

        [Fact]
        public void Product_type_should_be_set_to_active_energy()
        {
            var address = Address.Create(
                SampleData.StreetName,
                SampleData.StreetCode,
                string.Empty,
                SampleData.CityName,
                string.Empty,
                SampleData.PostCode,
                string.Empty,
                string.Empty,
                string.Empty,
                default);
            var meteringPointDetails = CreateDetails()
                with
                {
                    Address = address,
                };

            var meteringPoint = ConsumptionMeteringPoint.Create(meteringPointDetails);

            var createdEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is ConsumptionMeteringPointCreated) as ConsumptionMeteringPointCreated;

            Assert.Equal(ProductType.EnergyActive.Name, createdEvent!.ProductType);
        }

        private static MeteringPointDetails CreateDetails()
        {
            var address = Address.Create(
                SampleData.StreetName,
                SampleData.StreetCode,
                string.Empty,
                SampleData.CityName,
                string.Empty,
                SampleData.PostCode,
                string.Empty,
                string.Empty,
                string.Empty,
                default);

            return new MeteringPointDetails(
                MeteringPointId.New(),
                GsrnNumber.Create(SampleData.GsrnNumber),
                address,
                SampleData.IsOfficialAddress,
                MeteringPointSubType.Physical,
                GridAreaId.New(),
                GsrnNumber.Create(SampleData.PowerPlant),
                SampleData.LocationDescription,
                MeasurementUnitType.KWh,
                SampleData.MeterNumber,
                ReadingOccurrence.Hourly,
                SampleData.MaximumCurrent,
                SampleData.MaximumPower,
                EffectiveDate.Create(SampleData.EffectiveDate),
                SettlementMethod.Flex,
                NetSettlementGroup.Three,
                DisconnectionType.Manual,
                ConnectionType.Installation,
                AssetType.GasTurbine,
                ScheduledMeterReadingDate.Create(SampleData.ScheduledMeterReadingDate));
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
