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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.Consumption
{
    public class ChangeMasterDataTests : TestBase
    {
        [Fact]
        public void Should_return_error_when_street_name_is_blank()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = new MasterDataDetails(string.Empty);

            var result = meteringPoint.CanChange(details);

            AssertError<StreetNameIsRequiredRuleError>(result, true);
        }

        [Fact]
        public void Should_return_success_when_street_name_is_null()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = new MasterDataDetails(null);

            var result = meteringPoint.CanChange(details);

            Assert.True(result.Success);
        }

        [Fact]
        public void Should_throw_if_any_business_rule_are_violated()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = new MasterDataDetails(string.Empty);

            Assert.Throws<MasterDataChangeException>(() => meteringPoint.Change(details));
        }

        [Fact]
        public void Should_return_error_when_post_code_is_blank()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = new MasterDataDetails()
                with
                {
                    PostCode = string.Empty,
                };

            var result = meteringPoint.CanChange(details);

            AssertError<PostCodeIsRequiredRuleError>(result, true);
        }

        [Fact]
        public void Should_return_success_when_post_code_is_null()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = new MasterDataDetails(PostCode: null);

            var result = meteringPoint.CanChange(details);

            Assert.True(result.Success);
        }

        [Fact]
        public void Should_return_error_when_city_is_blank()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = new MasterDataDetails()
                with
                {
                    City = string.Empty,
                };

            var result = meteringPoint.CanChange(details);

            AssertError<CityIsRequiredRuleError>(result, true);
        }

        [Fact]
        public void Should_return_success_when_city_is_null()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = new MasterDataDetails(City: null);

            var result = meteringPoint.CanChange(details);

            Assert.True(result.Success);
        }

        [Fact]
        public void Should_change_address()
        {
            var meteringPoint = CreateMeteringPoint();
            var details = new MasterDataDetails(
                StreetName: "New Street Name",
                PostCode: "6000",
                City: "New City Name",
                StreetCode: "0500",
                BuildingNumber: "4",
                CitySubDivision: "New",
                CountryCode: CountryCode.DK,
                Floor: "9",
                Room: "9",
                MunicipalityCode: 999,
                IsActual: true,
                GeoInfoReference: Guid.NewGuid());

            meteringPoint.Change(details);

            var changeEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is AddressChanged) as AddressChanged;
            Assert.NotNull(changeEvent);
            Assert.Equal(details.City, changeEvent?.City);
            Assert.Equal(details.Floor, changeEvent?.Floor);
            Assert.Equal(details.Room, changeEvent?.Room);
            Assert.Equal(details.BuildingNumber, changeEvent?.BuildingNumber);
            Assert.Equal(details.CountryCode?.Name, changeEvent?.CountryCode);
            Assert.Equal(details.PostCode, changeEvent?.PostCode);
            Assert.Equal(details.StreetCode, changeEvent?.StreetCode);
            Assert.Equal(details.StreetName, changeEvent?.StreetName);
            Assert.Equal(details.CitySubDivision, changeEvent?.CitySubDivision);
            Assert.Equal(details.IsActual, changeEvent?.IsActual);
            Assert.Equal(details.MunicipalityCode, changeEvent?.MunicipalityCode);
            Assert.Equal(details.GeoInfoReference, changeEvent?.GeoInfoReference);
        }

        private static ConsumptionMeteringPoint CreateMeteringPoint()
        {
            return ConsumptionMeteringPoint.Create(CreateConsumptionDetails());
        }
    }
}
