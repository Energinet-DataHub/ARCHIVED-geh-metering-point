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
using Dapper;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    public class AssertPersistedMeteringPoint
    {
        private readonly dynamic _meteringPoint;

        private AssertPersistedMeteringPoint(dynamic meteringPoint)
        {
            if (meteringPoint == null) throw new ArgumentNullException(nameof(meteringPoint));
            Assert.NotNull(meteringPoint);
            _meteringPoint = meteringPoint;
        }

        public static AssertPersistedMeteringPoint Initialize(string gsrnNumber, IDbConnectionFactory connectionFactory)
        {
            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));
            var meteringPoint = connectionFactory.GetOpenConnection().QuerySingle(
                $"SELECT Id, RecordId, GsrnNumber, StreetName, PostCode, CityName, CountryCode," +
                $"MeteringPointSubType, MeterReadingOccurrence, TypeOfMeteringPoint, MaximumCurrent, MaximumPower," +
                $"MeteringGridArea, PowerPlant, LocationDescription, ProductType, ParentRelatedMeteringPoint," +
                $"UnitType, EffectiveDate, MeterNumber, ConnectionState_PhysicalState, ConnectionState_EffectiveDate," +
                $"StreetCode, CitySubDivision, Floor, Room, BuildingNumber, MunicipalityCode, IsActualAddress, GeoInfoReference," +
                $"Capacity, AssetType, SettlementMethod, ScheduledMeterReadingDate, ProductionObligation," +
                $"NetSettlementGroup, DisconnectionType, ConnectionType, StartOfSupplyDate, ToGrid, FromGrid" +
                $" FROM [dbo].[MeteringPoints]" +
                $" WHERE GsrnNumber = {gsrnNumber}");

            Assert.NotNull(meteringPoint);
            return new AssertPersistedMeteringPoint(meteringPoint);
        }

        public AssertPersistedMeteringPoint HasConnectionState(PhysicalState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));
            Assert.Equal(state.Name, _meteringPoint.ConnectionState_PhysicalState);
            return this;
        }

        public AssertPersistedMeteringPoint HasStreetName(string? streetName)
        {
            Assert.Equal(streetName, _meteringPoint.StreetName);
            return this;
        }

        public AssertPersistedMeteringPoint HasPostCode(string? postCode)
        {
            Assert.Equal(postCode, _meteringPoint.PostCode);
            return this;
        }

        public AssertPersistedMeteringPoint HasCity(string? city)
        {
            Assert.Equal(city, _meteringPoint.CityName);
            return this;
        }

        public AssertPersistedMeteringPoint HasStreetCode(string? streetCode)
        {
            Assert.Equal(streetCode, _meteringPoint.StreetCode);
            return this;
        }

        public AssertPersistedMeteringPoint HasBuildingNumber(string? buildingNumber)
        {
            Assert.Equal(buildingNumber, _meteringPoint.BuildingNumber);
            return this;
        }

        public AssertPersistedMeteringPoint HasCitySubDivision(string? citySubDivision)
        {
            Assert.Equal(citySubDivision, _meteringPoint.CitySubDivision);
            return this;
        }

        public AssertPersistedMeteringPoint HasCountryCode(CountryCode countryCode)
        {
            Assert.Equal(countryCode, EnumerationType.FromName<CountryCode>(_meteringPoint.CountryCode));
            return this;
        }

        public AssertPersistedMeteringPoint HasFloor(string? floor)
        {
            Assert.Equal(floor, _meteringPoint.Floor);
            return this;
        }

        public AssertPersistedMeteringPoint HasRoom(string? room)
        {
            Assert.Equal(room, _meteringPoint.Room);
            return this;
        }

        public AssertPersistedMeteringPoint HasMunicipalityCode(int? municipalityCode)
        {
            Assert.Equal(municipalityCode, _meteringPoint.MunicipalityCode);
            return this;
        }

        public AssertPersistedMeteringPoint HasIsActualAddress(bool? isActualAddress)
        {
            Assert.Equal(isActualAddress, _meteringPoint.IsActualAddress);
            return this;
        }

        public AssertPersistedMeteringPoint HasGeoInfoReference(Guid? geoInfoReference)
        {
            Assert.Equal(geoInfoReference, _meteringPoint.GeoInfoReference);
            return this;
        }
    }
}
