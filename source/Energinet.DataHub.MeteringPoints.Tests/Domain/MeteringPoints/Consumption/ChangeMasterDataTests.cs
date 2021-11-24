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
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Events;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.Consumption
{
    [UnitTest]
    public class ChangeMasterDataTests : TestBase
    {
        [Fact]
        public void Cannot_change_metering_configuration_when_metering_point_is_Closed_down()
        {
            var meteringPoint = CreateMeteringPoint();
            meteringPoint.CloseDown();

            Assert.Throws<MeteringPointClosedForChangesException>(() => meteringPoint.ChangeMeteringConfiguration(MeteringConfiguration.Create(MeteringMethod.Calculated, MeterId.Empty()), EffectiveDate.Create(SampleData.EffectiveDate)));
        }

        [Fact]
        public void Cannot_change_address_when_metering_point_is_closed_down()
        {
            var meteringPoint = CreateMeteringPoint();
            meteringPoint.CloseDown();

            Assert.Throws<MeteringPointClosedForChangesException>(() => meteringPoint.ChangeAddress(CreateAddress()));
        }

        [Fact]
        public void Can_not_change_when_metering_point_is_closed_down()
        {
            var meteringPoint = CreateMeteringPoint();
            meteringPoint.CloseDown();

            var result = meteringPoint.CanBeChanged();

            AssertError<ClosedDownMeteringPointCannotBeChangedError>(result, true);
        }

        [Fact]
        public void Should_return_error_when_street_name_is_blank()
        {
            var meteringPoint = CreateMeteringPoint();

            var result = meteringPoint.CanChangeAddress(Address.Create(
                city: SampleData.CityName,
                streetName: string.Empty,
                countryCode: CountryCode.DK,
                postCode: SampleData.PostCode));

            AssertError<StreetNameIsRequiredRuleError>(result, true);
        }

        [Fact]
        public void Should_throw_if_any_business_rule_are_violated()
        {
            var meteringPoint = CreateMeteringPoint();

            Assert.Throws<MasterDataChangeException>(() => meteringPoint.ChangeAddress(Address.Create(city: SampleData.CityName, streetName: string.Empty, countryCode: CountryCode.DK, postCode: SampleData.PostCode)));
        }

        [Fact]
        public void Should_return_error_when_post_code_is_blank()
        {
            var meteringPoint = CreateMeteringPoint();

            var result = meteringPoint.CanChangeAddress(Address.Create(city: SampleData.CityName, streetName: SampleData.StreetName, countryCode: CountryCode.DK, postCode: string.Empty));

            AssertError<PostCodeIsRequiredRuleError>(result, true);
        }

        [Fact]
        public void Should_return_error_when_city_is_blank()
        {
            var meteringPoint = CreateMeteringPoint();

            var result = meteringPoint.CanChangeAddress(Address.Create(city: null, streetName: SampleData.StreetName, countryCode: CountryCode.DK, postCode: SampleData.PostCode));

            AssertError<CityIsRequiredRuleError>(result, true);
        }

        [Fact]
        public void Should_change_address()
        {
            var meteringPoint = CreateMeteringPoint();
            var address = Address.Create(
                streetName: "New Street Name",
                postCode: "6000",
                city: "New City Name",
                streetCode: "0500",
                buildingNumber: "4",
                citySubDivision: "New",
                countryCode: CountryCode.DK,
                floor: "9",
                room: "9",
                municipalityCode: 999,
                isActual: true,
                geoInfoReference: Guid.NewGuid());

            meteringPoint.ChangeAddress(address);

            var changeEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is AddressChanged) as AddressChanged;
            Assert.NotNull(changeEvent);
            Assert.Equal(address.City, changeEvent?.City);
            Assert.Equal(address.Floor, changeEvent?.Floor);
            Assert.Equal(address.Room, changeEvent?.Room);
            Assert.Equal(address.BuildingNumber, changeEvent?.BuildingNumber);
            Assert.Equal(address.CountryCode?.Name, changeEvent?.CountryCode);
            Assert.Equal(address.PostCode, changeEvent?.PostCode);
            Assert.Equal(address.StreetCode, changeEvent?.StreetCode);
            Assert.Equal(address.StreetName, changeEvent?.StreetName);
            Assert.Equal(address.CitySubDivision, changeEvent?.CitySubDivision);
            Assert.Equal(address.IsActual, changeEvent?.IsActual);
            Assert.Equal(address.MunicipalityCode, changeEvent?.MunicipalityCode);
            Assert.Equal(address.GeoInfoReference, changeEvent?.GeoInfoReference);
        }

        [Fact]
        public void Metering_configuration_is_changed()
        {
            var meteringPoint = CreatePhysical();
            var effectiveDate = EffectiveDate.Create(SampleData.EffectiveDate);
            var configuration =
                MeteringConfiguration.Create(meteringPoint.MeteringConfiguration.Method, MeterId.Create("NewId"));

            meteringPoint.ChangeMeteringConfiguration(configuration, effectiveDate);

            var expectedEvent = FindDomainEvent<MeteringConfigurationChanged>(meteringPoint);
            Assert.Equal(configuration.Meter.Value, expectedEvent?.MeterId);
            Assert.Equal(configuration.Method.Name, expectedEvent?.MeteringMethod);
            Assert.Equal(effectiveDate.ToString(), expectedEvent?.EffectiveDate);
        }

        [Fact]
        public void Connection_type_is_not_applicable_for_net_settlement_group_0()
        {
            var meteringPoint = CreatePhysicalInNetSettlementGroup0();

            var result = meteringPoint.CanChangeConnectionType(ConnectionType.Direct);

            AssertError<ConnectionTypeIsNotAllowedRuleError>(result, true);
        }

        [Fact]
        public void Connection_type_is_installation_for_net_settlement_group_6()
        {
            var meteringPoint = CreateInNetSettlementGroup6();

            var result = meteringPoint.CanChangeConnectionType(ConnectionType.Direct);

            AssertError<ConnectionTypeDoesNotMatchNetSettlementGroupRuleError>(result, true);
        }

        [Fact]
        public void Connection_type_is_changed()
        {
            var meteringPoint = CreateInNetSettlementGroup1();

            meteringPoint.SetConnectionType(ConnectionType.Direct);

            var domainEvent = FindDomainEvent<ConnectionTypeChanged>(meteringPoint);

            Assert.Equal(ConnectionType.Direct.Name, domainEvent!.ConnectionType);
        }

        [Fact]
        public void Connection_type_cannot_be_changed()
        {
            var meteringPoint = CreateInNetSettlementGroup6();

            Assert.Throws<MasterDataChangeException>(() => meteringPoint.SetConnectionType(ConnectionType.Direct));
        }

        private static TDomainEvent? FindDomainEvent<TDomainEvent>(Entity domainEntity)
            where TDomainEvent : DomainEventBase
        {
            return domainEntity.DomainEvents.FirstOrDefault(e => e is TDomainEvent) as TDomainEvent;
        }

        private static ConsumptionMeteringPoint CreateMeteringPoint()
        {
            return ConsumptionMeteringPoint.Create(CreateConsumptionDetails());
        }

        private static ConsumptionMeteringPoint CreatePhysical()
        {
            var details = CreateConsumptionDetails()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.Zero,
                    ConnectionType = null,
                    MeteringConfiguration = MeteringConfiguration.Create(MeteringMethod.Physical, MeterId.Create("1")),
                };
            return ConsumptionMeteringPoint.Create(details);
        }

        private static ConsumptionMeteringPoint CreatePhysicalInNetSettlementGroup0()
        {
            var details = CreateConsumptionDetails()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.Zero,
                    ConnectionType = null,
                    MeteringConfiguration = MeteringConfiguration.Create(MeteringMethod.Physical, MeterId.Create("1")),
                };
            return ConsumptionMeteringPoint.Create(details);
        }

        private static ConsumptionMeteringPoint CreateInNetSettlementGroup6()
        {
            var details = CreateConsumptionDetails()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.Six,
                    ConnectionType = ConnectionType.Installation,
                    MeteringConfiguration = MeteringConfiguration.Create(MeteringMethod.Virtual, MeterId.Empty()),
                    ScheduledMeterReadingDate = ScheduledMeterReadingDate.Create("0101"),
                };
            return ConsumptionMeteringPoint.Create(details);
        }

        private static ConsumptionMeteringPoint CreateInNetSettlementGroup1()
        {
            var details = CreateConsumptionDetails()
                with
                {
                    NetSettlementGroup = NetSettlementGroup.One,
                    ConnectionType = ConnectionType.Installation,
                    MeteringConfiguration = MeteringConfiguration.Create(MeteringMethod.Virtual, MeterId.Empty()),
                    //ScheduledMeterReadingDate = ScheduledMeterReadingDate.Create("0101"),
                };
            return ConsumptionMeteringPoint.Create(details);
        }
    }
}
