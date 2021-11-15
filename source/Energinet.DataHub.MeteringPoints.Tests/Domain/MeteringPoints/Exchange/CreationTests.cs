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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Exchange;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.Exchange
{
    [UnitTest]
    public class CreationTests : TestBase
    {
        [Fact]
        public void Should_succeed()
        {
            var meteringPointId = MeteringPointId.New();
            var meteringPointGsrn = GsrnNumber.Create(SampleData.GsrnNumber);
            var meteringMethod = MeteringMethod.Virtual;
            var areadLinkId = new GridAreaLinkId(Guid.Parse(SampleData.GridAreaLinkId));
            var readingOccurrence = ReadingOccurrence.Hourly;
            var powerLimit = PowerLimit.Create(0, 0);
            var effectiveDate = EffectiveDate.Create(SampleData.EffectiveDate);
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
                isActual: false,
                geoInfoReference: SampleData.GeoInfoReference);

            var exchangeMeteringPointDetails = CreateExchangeDetails()
                with
                {
                    Id = meteringPointId,
                    Address = address,
                    GridAreaLinkId = areadLinkId,
                    PowerLimit = powerLimit,
                    MeterNumber = null,
                    MeteringMethod = meteringMethod,
                };

            var meteringPoint = ExchangeMeteringPoint.Create(exchangeMeteringPointDetails);

            var createdEvent = meteringPoint.DomainEvents.First(e => e is ExchangeMeteringPointCreated) as ExchangeMeteringPointCreated;
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
            Assert.Equal(meteringPointId.Value, createdEvent.MeteringPointId);
            Assert.Equal(meteringPointGsrn.Value, createdEvent.GsrnNumber);
            Assert.Equal(meteringMethod.Name, createdEvent.MeteringPointSubType);
            Assert.Equal(areadLinkId.Value, createdEvent.GridAreaLinkId);
            Assert.Equal(address.LocationDescription, createdEvent.LocationDescription);
            Assert.Equal(readingOccurrence.Name, createdEvent.ReadingOccurrence);
            Assert.Equal(powerLimit.Ampere, createdEvent.MaximumCurrent);
            Assert.Equal(powerLimit.Kwh, createdEvent.MaximumPower);
            Assert.Equal(effectiveDate.DateInUtc, createdEvent.EffectiveDate);
        }

        [Fact]
        public void Product_type_should_as_default_be_active_energy()
        {
            var details = CreateExchangeDetails()
            with
            {
                MeteringMethod = MeteringMethod.Virtual,
                MeterNumber = null,
            };

            var meteringPoint = ExchangeMeteringPoint.Create(details);

            var createdEvent = meteringPoint.DomainEvents.First(e => e is ExchangeMeteringPointCreated) as ExchangeMeteringPointCreated;
            Assert.Equal(ProductType.EnergyActive.Name, createdEvent!.ProductType);
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
                geoInfoReference: null);

            var meteringPointDetails = CreateExchangeDetails()
                with
                {
                    Address = address,
                };

            var checkResult = CheckCreationRules(meteringPointDetails);
            AssertContainsValidationError<StreetNameIsRequiredRuleError>(checkResult);
        }

        [Fact]
        public void Product_type_should_be_set_to_active_energy()
        {
            var address = CreateAddress();

            var meteringPointDetails = CreateExchangeDetails()
                with
                {
                    Address = address,
                    MeteringMethod = MeteringMethod.Virtual,
                    MeterNumber = null,
                };

            var meteringPoint = ExchangeMeteringPoint.Create(meteringPointDetails);

            var createdEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is ExchangeMeteringPointCreated) as ExchangeMeteringPointCreated;

            Assert.Equal(ProductType.EnergyActive.Name, createdEvent!.ProductType);
        }

        private static BusinessRulesValidationResult CheckCreationRules(ExchangeMeteringPointDetails meteringPointDetails)
        {
            return ExchangeMeteringPoint.CanCreate(meteringPointDetails);
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
