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
using Energinet.DataHub.MeteringPoints.Domain.Actors;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;
using Xunit.Categories;
using MeteringPointCreated = Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events.MeteringPointCreated;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.MeteringPoints.Exchange
{
    [UnitTest]
    public class CreationTests : TestBase
    {
        [Fact]
        public void Should_succeed()
        {
            var sourceGridAreaLinkId = GridAreaLinkId.New();
            var targetGridAreaLinkId = GridAreaLinkId.New();
            var meteringConfiguration = MeteringConfiguration.Create(MeteringMethod.Virtual, MeterId.Empty());
            var meteringPointId = MeteringPointId.New();
            var meteringPointGsrn = GsrnNumber.Create(SampleData.GsrnNumber);
            var meteringMethod = MeteringMethod.Virtual;
            var gridAreaLinkId = new GridAreaLinkId(Guid.Parse(SampleData.GridAreaLinkId));
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

            var meteringPoint = MeteringPoint.CreateExchange(
                meteringPointId,
                meteringPointGsrn,
                gridAreaLinkId,
                ExchangeGridAreas.Create(sourceGridAreaLinkId, targetGridAreaLinkId),
                ActorId.Create(),
                MasterDataBuilderForExchange()
                .WithAddress(address.StreetName, address.StreetCode, address.BuildingNumber, address.City, address.CitySubDivision, address.PostCode, address.CountryCode, address.Floor, address.Room, address.MunicipalityCode, address.IsActual, address.GeoInfoReference.ToString(), address.LocationDescription)
                .Build());

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
            Assert.Equal(meteringPointId.Value, createdEvent.MeteringPointId);
            Assert.Equal(meteringPointGsrn.Value, createdEvent.GsrnNumber);
            Assert.Equal(meteringConfiguration.Method.Name, createdEvent.MeteringPointSubType);
            Assert.Equal(gridAreaLinkId.Value, createdEvent.GridAreaLinkId);
            Assert.Equal(address.LocationDescription, createdEvent.LocationDescription);
            Assert.Equal(readingOccurrence.Name, createdEvent.ReadingOccurrence);
            Assert.Equal(powerLimit.Ampere, createdEvent.MaximumCurrent);
            Assert.Equal(powerLimit.Kwh, createdEvent.MaximumPower);
            Assert.Equal(effectiveDate.DateInUtc, createdEvent.EffectiveDate);
            Assert.Equal(targetGridAreaLinkId.Value, createdEvent.TargetGridAreaLinkId);
            Assert.Equal(sourceGridAreaLinkId.Value, createdEvent.SourceGridAreaLinkId);
        }
    }
}
