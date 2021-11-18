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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Production.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Special;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.Special
{
    [UnitTest]
    public class CreationTests : TestBase
    {
        [Fact]
        public void Should_succeed()
        {
            var meteringPointId = MeteringPointId.New();
            var meteringPointGsrn = GsrnNumber.Create(SampleData.GsrnNumber);
            var isActualAddress = SampleData.IsActualAddress;
            var gridAreaLinkId = new GridAreaLinkId(Guid.Parse(SampleData.GridAreaLinkId));
            var powerPlantGsrn = GsrnNumber.Create(SampleData.PowerPlant);
            var measurementUnitType = MeasurementUnitType.KWh;
            var readingOccurrence = ReadingOccurrence.Hourly;
            var powerLimit = PowerLimit.Create(0, 0);
            var effectiveDate = EffectiveDate.Create(SampleData.EffectiveDate);
            var assetType = AssetType.GasTurbine;
            var meteringConfiguration = MeteringConfiguration.Create(MeteringMethod.Virtual, MeterId.Empty());
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
            var capacity = Capacity.Create(SampleData.Capacity);

            var specialMeteringPointDetails = CreateSpecialDetails()
                with
                {
                    Id = meteringPointId,
                    Address = address,
                    GridAreaLinkId = gridAreaLinkId,
                    PowerLimit = powerLimit,
                    Capacity = capacity,
                    MeteringConfiguration = meteringConfiguration,
                };

            var meteringPoint = SpecialMeteringPoint.Create(specialMeteringPointDetails);

            var createdEvent = meteringPoint.DomainEvents.First(e => e is SpecialMeteringPointCreated) as SpecialMeteringPointCreated;
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
            Assert.Equal(meteringConfiguration.Method.Name, createdEvent.MeteringPointSubType);
            Assert.Equal(gridAreaLinkId.Value, createdEvent.GridAreaLinkId);
            Assert.Equal(powerPlantGsrn.Value, createdEvent.PowerPlantGsrnNumber);
            Assert.Equal(measurementUnitType.Name, createdEvent.UnitType);
            Assert.Equal(readingOccurrence.Name, createdEvent.ReadingOccurrence);
            Assert.Equal(powerLimit.Ampere, createdEvent.MaximumCurrent);
            Assert.Equal(powerLimit.Kwh, createdEvent.MaximumPower);
            Assert.Equal(effectiveDate.DateInUtc, createdEvent.EffectiveDate);
            Assert.Equal(assetType.Name, createdEvent.AssetType);
            Assert.Equal(capacity.Kw, createdEvent.Capacity);
        }

        [Fact]
        public void Product_type_should_as_default_be_active_energy()
        {
            var details = CreateSpecialDetails()
            with
            {
                MeteringConfiguration = MeteringConfiguration.Create(MeteringMethod.Virtual, MeterId.Empty()),
            };

            var meteringPoint = SpecialMeteringPoint.Create(details);

            var createdEvent = meteringPoint.DomainEvents.First(e => e is SpecialMeteringPointCreated) as SpecialMeteringPointCreated;
            Assert.Equal(ProductType.EnergyActive.Name, createdEvent!.ProductType);
        }

        [Fact]
        public void Product_type_should_be_set_to_active_energy()
        {
            var address = CreateAddress();

            var meteringPointDetails = CreateSpecialDetails()
                with
                {
                    Address = address,
                    MeteringConfiguration = MeteringConfiguration.Create(MeteringMethod.Virtual, MeterId.Empty()),
                };

            var meteringPoint = SpecialMeteringPoint.Create(meteringPointDetails);

            var createdEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is SpecialMeteringPointCreated) as SpecialMeteringPointCreated;

            Assert.Equal(ProductType.EnergyActive.Name, createdEvent!.ProductType);
        }

        [Fact]
        public void Power_plant_should_not_be_required()
        {
            var meteringPointDetails = CreateSpecialDetails()
                with
                {
                    PowerPlantGsrnNumber = null,
                };

            var checkResult = CheckCreationRules(meteringPointDetails);

            AssertDoesNotContainValidationError<PowerPlantRequirementRuleError>(checkResult);
        }

        [Theory]
        [InlineData(nameof(ReadingOccurrence.Quarterly), false)]
        [InlineData(nameof(ReadingOccurrence.Hourly), false)]
        [InlineData(nameof(ReadingOccurrence.Monthly), false)]
        [InlineData(nameof(ReadingOccurrence.Yearly), true)]
        public void Meter_reading_occurrence_must_be_quarterly_hourly_or_monthly(string readingOccurrence, bool expectError)
        {
            var details = CreateSpecialDetails()
                with
                {
                    ReadingOccurrence = EnumerationType.FromName<ReadingOccurrence>(readingOccurrence),
                };

            var checkResult = CheckCreationRules(details);

            AssertError<InvalidMeterReadingOccurrenceRuleError>(checkResult, expectError);
        }

        private static BusinessRulesValidationResult CheckCreationRules(SpecialMeteringPointDetails meteringPointDetails)
        {
            return SpecialMeteringPoint.CanCreate(meteringPointDetails);
        }
    }
}
