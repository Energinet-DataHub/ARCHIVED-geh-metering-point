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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.Consumption
{
    [UnitTest]
    public class ChangeMasterDataTests : TestBase
    {
        [Fact]
        public void Should_return_error_when_street_name_is_blank()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = CreateDetails()
                with
                {
                    Address = Address.Create(
                        city: SampleData.CityName,
                        streetName: string.Empty,
                        countryCode: CountryCode.DK,
                        postCode: SampleData.PostCode),
                };

            var result = meteringPoint.CanChange(details);

            AssertError<StreetNameIsRequiredRuleError>(result, true);
        }

        [Fact]
        public void Should_throw_if_any_business_rule_are_violated()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = CreateDetails()
                with
                {
                    Address = Address.Create(city: SampleData.CityName, streetName: string.Empty, countryCode: CountryCode.DK, postCode: SampleData.PostCode),
                };

            Assert.Throws<MasterDataChangeException>(() => meteringPoint.Change(details));
        }

        [Fact]
        public void Should_return_error_when_post_code_is_blank()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = CreateDetails()
                with
                {
                    Address = Address.Create(city: SampleData.CityName, streetName: SampleData.StreetName, countryCode: CountryCode.DK, postCode: string.Empty),
                };
            var result = meteringPoint.CanChange(details);

            AssertError<PostCodeIsRequiredRuleError>(result, true);
        }

        [Fact]
        public void Should_return_error_when_city_is_blank()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = CreateDetails()
                with
                {
                    Address = Address.Create(city: null, streetName: SampleData.StreetName, countryCode: CountryCode.DK, postCode: SampleData.PostCode),
                };

            var result = meteringPoint.CanChange(details);

            AssertError<CityIsRequiredRuleError>(result, true);
        }

        [Fact]
        public void Should_change_address()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = CreateDetails()
                with
                {
                    Address = Address.Create(
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
                        geoInfoReference: Guid.NewGuid()),
                };

            meteringPoint.Change(details);

            var changeEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is AddressChanged) as AddressChanged;
            Assert.NotNull(changeEvent);
            Assert.Equal(details.Address?.City, changeEvent?.City);
            Assert.Equal(details.Address?.Floor, changeEvent?.Floor);
            Assert.Equal(details.Address?.Room, changeEvent?.Room);
            Assert.Equal(details.Address?.BuildingNumber, changeEvent?.BuildingNumber);
            Assert.Equal(details.Address?.CountryCode?.Name, changeEvent?.CountryCode);
            Assert.Equal(details.Address?.PostCode, changeEvent?.PostCode);
            Assert.Equal(details.Address?.StreetCode, changeEvent?.StreetCode);
            Assert.Equal(details.Address?.StreetName, changeEvent?.StreetName);
            Assert.Equal(details.Address?.CitySubDivision, changeEvent?.CitySubDivision);
            Assert.Equal(details.Address?.IsActual, changeEvent?.IsActual);
            Assert.Equal(details.Address?.MunicipalityCode, changeEvent?.MunicipalityCode);
            Assert.Equal(details.Address?.GeoInfoReference, changeEvent?.GeoInfoReference);
        }

        [Fact]
        public void Meter_id_is_required_when_physical()
        {
            var meteringPoint = CreatePhysical();
            var details = CreateDetails()
                with
                {
                    MeterId = MeterId.NotSet(),
                };

            var result = meteringPoint.CanChange(details);

            AssertError<MeterIdIsRequiredRuleError>(result, true);
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
                    MeteringMethod = MeteringMethod.Physical,
                    MeterNumber = MeterId.Create("1"),
                    NetSettlementGroup = NetSettlementGroup.Zero,
                    ConnectionType = null,
                };
            return ConsumptionMeteringPoint.Create(details);
        }

        private static MasterDataDetails CreateDetails()
        {
            return new MasterDataDetails(
                EffectiveDate: EffectiveDate.Create(SampleData.EffectiveDate),
                Address: Address.Create(city: null, streetName: SampleData.StreetName, countryCode: CountryCode.DK, postCode: SampleData.PostCode));
        }
    }
}
