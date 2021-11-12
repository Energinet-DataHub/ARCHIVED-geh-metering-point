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
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.Consumption
{
    [UnitTest]
    public class CreationTests : TestBase
    {
        [Theory]
        [InlineData("Zero", "Physical", false)]
        [InlineData("One", "Physical", true)]
        [InlineData("One", "Virtual", false)]
        [InlineData("One", "Calculated", false)]
        [InlineData("Two", "Physical", true)]
        [InlineData("Two", "Virtual", false)]
        [InlineData("Two", "Calculated", false)]
        [InlineData("Three", "Physical", true)]
        [InlineData("Three", "Virtual", false)]
        [InlineData("Three", "Calculated", false)]
        [InlineData("Six", "Physical", true)]
        [InlineData("Six", "Virtual", false)]
        [InlineData("Six", "Calculated", false)]
        [InlineData("NinetyNine", "Physical", false)]
        public void Metering_method_must_be_virtual_or_calculated_when_net_settlement_group_is_not_0_or_99(string netSettlementGroup, string meteringMethod, bool expectError)
        {
            var method = EnumerationType.FromName<MeteringMethod>(meteringMethod);
            var meter = method == MeteringMethod.Physical ? MeterId.Create("Fake") : MeterId.Empty();
            var details = CreateConsumptionDetails()
                with
                {
                    NetSettlementGroup = EnumerationType.FromName<NetSettlementGroup>(netSettlementGroup),
                    MeteringConfiguration = MeteringConfiguration.Create(method, meter),
                };

            var result = ConsumptionMeteringPoint.CanCreate(details);

            AssertError<MeteringMethodDoesNotMatchNetSettlementGroupRuleError>(result, expectError);
        }

        [Fact]
        public void Should_succeed()
        {
            var meteringPointId = MeteringPointId.New();
            var meteringPointGsrn = GsrnNumber.Create(SampleData.GsrnNumber);
            var isActualAddress = SampleData.IsActualAddress;
            var meteringMethod = MeteringMethod.Virtual;
            var gridAreaLinkId = new GridAreaLinkId(Guid.Parse(SampleData.GridAreaLinkId));
            var powerPlantGsrn = GsrnNumber.Create(SampleData.PowerPlant);
            var netSettlementGroup = NetSettlementGroup.Zero;
            var locationDescription = LocationDescription.Create(string.Empty);
            var measurementUnitType = MeasurementUnitType.KWh;
            var readingOccurrence = ReadingOccurrence.Hourly;
            var powerLimit = PowerLimit.Create(0, 0);
            var effectiveDate = EffectiveDate.Create(SampleData.EffectiveDate);
            var settlementMethod = SettlementMethod.Flex;
            var disconnectionType = DisconnectionType.Manual;
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
                isActual: isActualAddress,
                geoInfoReference: Guid.NewGuid());
            var scheduledMeterReadingDate = ScheduledMeterReadingDate.Create("0101");
            var capacity = Capacity.Create(SampleData.Capacity);

            var consumptionMeteringPointDetails = CreateConsumptionDetails()
                with
                {
                    Id = meteringPointId,
                    Address = address,
                    GridAreaLinkId = gridAreaLinkId,
                    LocationDescription = locationDescription,
                    PowerLimit = powerLimit,
                    DisconnectionType = disconnectionType,
                    ConnectionType = null,
                    ScheduledMeterReadingDate = null,
                    Capacity = capacity,
                    NetSettlementGroup = netSettlementGroup,
                };

            var meteringPoint = ConsumptionMeteringPoint.Create(consumptionMeteringPointDetails);

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
            Assert.Equal(address.IsActual, createdEvent.IsActualAddress);
            Assert.Equal(address.GeoInfoReference, createdEvent.GeoInfoReference);
            Assert.Equal(meteringPointId.Value, createdEvent.MeteringPointId);
            Assert.Equal(meteringPointGsrn.Value, createdEvent.GsrnNumber);
            Assert.Equal(meteringMethod.Name, createdEvent.MeteringPointSubType);
            Assert.Equal(gridAreaLinkId.Value, createdEvent.GridAreaLinkId);
            Assert.Equal(consumptionMeteringPointDetails.NetSettlementGroup.Name, createdEvent.NetSettlementGroup);
            Assert.Equal(powerPlantGsrn.Value, createdEvent.PowerPlantGsrnNumber);
            Assert.Equal(locationDescription.Value, createdEvent.LocationDescription);
            Assert.Equal(measurementUnitType.Name, createdEvent.UnitType);
            Assert.Equal(readingOccurrence.Name, createdEvent.ReadingOccurrence);
            Assert.Equal(powerLimit.Ampere, createdEvent.MaximumCurrent);
            Assert.Equal(powerLimit.Kwh, createdEvent.MaximumPower);
            Assert.Equal(effectiveDate.DateInUtc, createdEvent.EffectiveDate);
            Assert.Equal(settlementMethod.Name, createdEvent.SettlementMethod);
            Assert.Equal(disconnectionType.Name, createdEvent.DisconnectionType);
            Assert.Equal(assetType.Name, createdEvent.AssetType);
            Assert.Equal(capacity.Kw, createdEvent.Capacity);
        }

        [Fact]
        public void Day_of_scheduled_meter_reading_date_must_be_01_when_net_settlement_group_is_6()
        {
            var details = CreateConsumptionDetails()
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
            var details = CreateConsumptionDetails()
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
            var details = CreateConsumptionDetails()
            with
            {
                MeteringConfiguration = MeteringConfiguration.Create(MeteringMethod.Virtual, MeterId.Empty()),
                ScheduledMeterReadingDate = null,
            };

            var meteringPoint = ConsumptionMeteringPoint.Create(details);

            var createdEvent = meteringPoint.DomainEvents.First(e => e is ConsumptionMeteringPointCreated) as ConsumptionMeteringPointCreated;
            Assert.Equal(ProductType.EnergyActive.Name, createdEvent!.ProductType);
        }

        [Fact]
        public void Powerplant_GSRN_is_required_when_netsettlementgroup_is_other_than_0_or_99()
        {
            var meteringPointDetails = CreateConsumptionDetails()
                with
                {
                    PowerPlantGsrnNumber = null,
                    NetSettlementGroup = NetSettlementGroup.Six,
                    MeteringConfiguration = MeteringConfiguration.Create(MeteringMethod.Virtual, MeterId.Empty()),
                };

            var checkResult = CheckCreationRules(meteringPointDetails);

            AssertContainsValidationError<PowerPlantIsRequiredForNetSettlementGroupRuleError>(checkResult);
        }

        [Theory]
        [InlineData(nameof(NetSettlementGroup.Zero))]
        [InlineData(nameof(NetSettlementGroup.Ninetynine))]
        public void Powerplant_GSRN_is_not_required_when_netsettlementgroup_is_0_or_99(string netSettlementGroupName)
        {
            var netSettlementGroup = EnumerationType.FromName<NetSettlementGroup>(netSettlementGroupName);
            var meteringPointDetails = CreateConsumptionDetails()
                with
                {
                    NetSettlementGroup = netSettlementGroup,
                };

            var checkResult = CheckCreationRules(meteringPointDetails);

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
                isActual: true,
                geoInfoReference: Guid.NewGuid());

            var meteringPointDetails = CreateConsumptionDetails()
                with
                {
                    Address = address,
                };

            var checkResult = CheckCreationRules(meteringPointDetails);
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
                isActual: true,
                geoInfoReference: Guid.NewGuid());

            var meteringPointDetails = CreateConsumptionDetails()
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

            var meteringPointDetails = CreateConsumptionDetails()
                with
                {
                    Address = address,
                    NetSettlementGroup = NetSettlementGroup.Six,
                    MeteringConfiguration = MeteringConfiguration.Create(MeteringMethod.Virtual, MeterId.Empty()),
                    ScheduledMeterReadingDate = ScheduledMeterReadingDate.Create("0101"),
                };

            var meteringPoint = ConsumptionMeteringPoint.Create(meteringPointDetails);

            var createdEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is ConsumptionMeteringPointCreated) as ConsumptionMeteringPointCreated;

            Assert.Equal(ProductType.EnergyActive.Name, createdEvent!.ProductType);
        }

        [Fact]
        public void Capacity_is_required_for_all_net_settlement_groups_but_0()
        {
            var details = CreateConsumptionDetails()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.Six,
                    Capacity = null,
                };

            var checkResult = CheckCreationRules(details);

            AssertContainsValidationError<CapacityIsRequiredRuleError>(checkResult);
        }

        [Fact]
        public void Asset_type_is_required_for_net_settlement_groups_other_than_0()
        {
            var details = CreateConsumptionDetails()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.Six,
                    AssetType = null,
                };

            var checkResult = CheckCreationRules(details);

            AssertContainsValidationError<AssetTypeIsRequiredRuleError>(checkResult);
        }

        [Theory]
        [InlineData("Profiled", true)]
        [InlineData("NonProfiled", false)]
        [InlineData("Flex", false)]
        public void Settlement_method_must_be_flex_or_non_profiled(string settlementMethod, bool expectError)
        {
            var details = CreateConsumptionDetails()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.Six,
                    SettlementMethod = EnumerationType.FromName<SettlementMethod>(settlementMethod),
                };

            var checkResult = CheckCreationRules(details);

            AssertError<InvalidSettlementMethodRuleError>(checkResult, expectError);
        }

        private static BusinessRulesValidationResult CheckCreationRules(ConsumptionMeteringPointDetails meteringPointDetails)
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
