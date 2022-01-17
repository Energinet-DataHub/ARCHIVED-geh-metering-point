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
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Xunit;
using Xunit.Categories;
using MeteringPointCreated = Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events.MeteringPointCreated;

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
            var isActualAddress = SampleData.IsActualAddress;
            var meteringMethod = MeteringMethod.Virtual;
            var gridAreaLinkId = new GridAreaLinkId(Guid.Parse(SampleData.GridAreaLinkId));
            var powerPlantGsrn = GsrnNumber.Create(SampleData.PowerPlant);
            var netSettlementGroup = NetSettlementGroup.Zero;
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

            var masterData = MasterDataBuilder(MeteringPointType.Consumption)
                .WithMeasurementUnitType(MeasurementUnitType.KWh.Name)
                .WithProductType(ProductType.EnergyActive.Name)
                .WithCapacity(capacity.ToString())
                .WithAddress(address.StreetName, address.StreetCode, address.BuildingNumber, address.City, address.CitySubDivision, address.PostCode, address.CountryCode, address.Floor, address.Room, address.MunicipalityCode, address.IsActual, address.GeoInfoReference, address.LocationDescription)
                .WithNetSettlementGroup(netSettlementGroup.Name)
                .Build();
            var meteringPoint = MeteringPoint.Create(meteringPointId, meteringPointGsrn, MeteringPointType.Consumption, gridAreaLinkId, masterData);

            var createdEvent = meteringPoint.DomainEvents.First(e => e is MeteringPointCreated) as MeteringPointCreated;
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
            Assert.Equal(masterData.NetSettlementGroup?.Name, createdEvent.NetSettlementGroup);
            Assert.Equal(powerPlantGsrn.Value, createdEvent.PowerPlantGsrnNumber);
            Assert.Equal(address.LocationDescription, createdEvent.LocationDescription);
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
    }
}
