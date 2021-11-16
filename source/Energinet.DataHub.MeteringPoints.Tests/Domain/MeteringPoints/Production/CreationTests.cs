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
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Production;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Production.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.Production
{
    [UnitTest]
    public class CreationTests : TestBase
    {
        [Fact]
        public void Should_succeed()
        {
            var meteringPointId = MeteringPointId.New();
            var meteringPointGsrn = GsrnNumber.Create(SampleData.GsrnNumber);
            var isOfficielAddress = SampleData.IsActualAddress;
            var meteringMethod = MeteringMethod.Virtual;
            var gridAreadLinkId = new GridAreaLinkId(Guid.Parse(SampleData.GridAreaLinkId));
            var powerPlanGsrn = GsrnNumber.Create(SampleData.PowerPlant);
            var netSettlementGroup = NetSettlementGroup.Zero;
            var measurementUnitType = MeasurementUnitType.KWh;
            var readingOccurrence = ReadingOccurrence.Hourly;
            var powerLimit = PowerLimit.Create(0, 0);
            var effectiveDate = EffectiveDate.Create(SampleData.EffectiveDate);
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
                isActual: true,
                geoInfoReference: Guid.NewGuid(),
                locationDescription: string.Empty);
            var capacity = Capacity.Create(SampleData.Capacity);

            var productionMeteringPointDetails = CreateProductionDetails()
                with
                {
                    Id = meteringPointId,
                    Address = address,
                    GridAreaLinkId = gridAreadLinkId,
                    PowerLimit = powerLimit,
                    DisconnectionType = disconnectionType,
                    ConnectionType = null,
                    Capacity = capacity,
                    NetSettlementGroup = netSettlementGroup,
                    MeterNumber = null,
                    MeteringMethod = meteringMethod,
                };

            var meteringPoint = ProductionMeteringPoint.Create(productionMeteringPointDetails);

            var createdEvent = meteringPoint.DomainEvents.First(e => e is ProductionMeteringPointCreated) as ProductionMeteringPointCreated;
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
            Assert.Equal(address.IsActual, createdEvent.IsOfficialAddress);
            Assert.Equal(address.GeoInfoReference, createdEvent.GeoInfoReference);
            Assert.Equal(address.LocationDescription, createdEvent.LocationDescription);
            Assert.Equal(meteringPointId.Value, createdEvent.MeteringPointId);
            Assert.Equal(meteringPointGsrn.Value, createdEvent.GsrnNumber);
            Assert.Equal(isOfficielAddress, createdEvent.IsOfficialAddress);
            Assert.Equal(meteringMethod.Name, createdEvent.MeteringPointSubType);
            Assert.Equal(gridAreadLinkId.Value, createdEvent.GridAreaLinkId);
            Assert.Equal(productionMeteringPointDetails.NetSettlementGroup.Name, createdEvent.NetSettlementGroup);
            Assert.Equal(powerPlanGsrn.Value, createdEvent.PowerPlantGsrnNumber);
            Assert.Equal(address.LocationDescription, createdEvent.LocationDescription);
            Assert.Equal(measurementUnitType.Name, createdEvent.UnitType);
            Assert.Equal(readingOccurrence.Name, createdEvent.ReadingOccurrence);
            Assert.Equal(powerLimit.Ampere, createdEvent.MaximumCurrent);
            Assert.Equal(powerLimit.Kwh, createdEvent.MaximumPower);
            Assert.Equal(effectiveDate.DateInUtc, createdEvent.EffectiveDate);
            Assert.Equal(disconnectionType.Name, createdEvent.DisconnectionType);
            Assert.Equal(assetType.Name, createdEvent.AssetType);
            Assert.Equal(capacity.Kw, createdEvent.Capacity);
            Assert.False(createdEvent.ProductionObligation);
        }

        [Fact]
        public void Product_type_should_as_default_be_active_energy()
        {
            var details = CreateProductionDetails()
            with
            {
                MeteringMethod = MeteringMethod.Virtual,
                MeterNumber = null,
            };

            var meteringPoint = ProductionMeteringPoint.Create(details);

            var createdEvent = meteringPoint.DomainEvents.First(e => e is ProductionMeteringPointCreated) as ProductionMeteringPointCreated;
            Assert.Equal(ProductType.EnergyActive.Name, createdEvent!.ProductType);
        }

        [Fact]
        public void Asset_type_is_required()
        {
            var details = CreateProductionDetails()
                with
                {
                   AssetType = null!,
                };

            var checkResult = CheckCreationRules(details);

            AssertContainsValidationError<AssetTypeIsRequiredRuleError>(checkResult);
        }

        [Fact]
        public void Powerplant_is_required()
        {
            var details = CreateProductionDetails()
                with
                {
                    PowerPlantGsrnNumber = null!,
                };

            var checkResult = CheckCreationRules(details);

            AssertContainsValidationError<PowerPlantRequirementRuleError>(checkResult);
        }

        [Fact]
        public void Product_type_should_be_set_to_active_energy()
        {
            var address = CreateAddress();

            var meteringPointDetails = CreateProductionDetails()
                with
                {
                    Address = address,
                    NetSettlementGroup = NetSettlementGroup.Six,
                    MeteringMethod = MeteringMethod.Virtual,
                    MeterNumber = null,
                };

            var meteringPoint = ProductionMeteringPoint.Create(meteringPointDetails);

            var createdEvent = meteringPoint.DomainEvents.FirstOrDefault(e => e is ProductionMeteringPointCreated) as ProductionMeteringPointCreated;

            Assert.Equal(ProductType.EnergyActive.Name, createdEvent!.ProductType);
        }

        private static BusinessRulesValidationResult CheckCreationRules(ProductionMeteringPointDetails meteringPointDetails)
        {
            return ProductionMeteringPoint.CanCreate(meteringPointDetails);
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
