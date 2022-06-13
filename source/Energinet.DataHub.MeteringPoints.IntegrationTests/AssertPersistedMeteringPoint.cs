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
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    public class AssertPersistedMeteringPoint
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly dynamic _meteringPoint;

        private AssertPersistedMeteringPoint(dynamic meteringPoint, IDbConnectionFactory connectionFactory)
        {
            if (meteringPoint == null) throw new ArgumentNullException(nameof(meteringPoint));
            Assert.NotNull(meteringPoint);
            _meteringPoint = meteringPoint;
            _connectionFactory = connectionFactory;
        }

        public static AssertPersistedMeteringPoint Initialize(string gsrnNumber, IDbConnectionFactory connectionFactory)
        {
            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));
            var meteringPoint = connectionFactory.GetOpenConnection().QuerySingle(
                $"SELECT mp.Id, mp.RecordId, mp.GsrnNumber, mp.StreetName, mp.PostCode, mp.CityName, mp.CountryCode," +
                $"mp.MeteringPointSubType, mp.MeterReadingOccurrence, mp.TypeOfMeteringPoint, mp.MaximumCurrent, mp.MaximumPower," +
                $"mp.MeteringGridArea, mp.PowerPlant, mp.LocationDescription, mp.ProductType, mp.ParentRelatedMeteringPoint," +
                $"mp.UnitType, mp.EffectiveDate, mp.MeterNumber, mp.ConnectionState_PhysicalState, mp.ConnectionState_EffectiveDate," +
                $"mp.StreetCode, mp.CitySubDivision, mp.Floor, mp.Room, mp.BuildingNumber, mp.MunicipalityCode, mp.IsActualAddress, mp.GeoInfoReference," +
                $"mp.Capacity, mp.AssetType, mp.SettlementMethod, mp.ScheduledMeterReadingDate, mp.ProductionObligation," +
                $"mp.NetSettlementGroup, mp.DisconnectionType, mp.ConnectionType, mp.StartOfSupplyDate, mp.ToGrid, mp.FromGrid" +
                $" FROM [dbo].[MeteringPoints] mp" +
                $" WHERE mp.GsrnNumber = {gsrnNumber}");

            Assert.NotNull(meteringPoint);
            return new AssertPersistedMeteringPoint(meteringPoint, connectionFactory);
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

        public AssertPersistedMeteringPoint HasProductType(ProductType productType)
        {
            Assert.Equal(productType, EnumerationType.FromName<ProductType>(_meteringPoint.ProductType));
            return this;
        }

        public AssertPersistedMeteringPoint HasUnitType(MeasurementUnitType measurementUnitType)
        {
            Assert.Equal(measurementUnitType, EnumerationType.FromName<MeasurementUnitType>(_meteringPoint.UnitType));
            return this;
        }

        public AssertPersistedMeteringPoint HasAssetType(AssetType assetType)
        {
            Assert.Equal(assetType, EnumerationType.FromName<AssetType>(_meteringPoint.AssetType));
            return this;
        }

        public AssertPersistedMeteringPoint HasReadingOccurrence(ReadingOccurrence readingOccurrence)
        {
            Assert.Equal(readingOccurrence, EnumerationType.FromName<ReadingOccurrence>(_meteringPoint.MeterReadingOccurrence));
            return this;
        }

        public AssertPersistedMeteringPoint HasPowerLimit(int? kwh, int? ampere)
        {
            Assert.Equal(kwh, _meteringPoint.MaximumPower);
            Assert.Equal(ampere, _meteringPoint.MaximumCurrent);
            return this;
        }

        public AssertPersistedMeteringPoint HasPowerPlantGsrnNumber(string powerPlantGsrnNumber)
        {
            Assert.Equal(powerPlantGsrnNumber, _meteringPoint.PowerPlant);
            return this;
        }

        public AssertPersistedMeteringPoint HasEffectiveDate(EffectiveDate effectiveDate)
        {
            Assert.Equal(effectiveDate, EffectiveDate.Create(_meteringPoint.EffectiveDate));
            return this;
        }

        public AssertPersistedMeteringPoint HasCapacity(int capacityInKw)
        {
            Assert.Equal(capacityInKw, _meteringPoint.Capacity);
            return this;
        }

        public AssertPersistedMeteringPoint HasMeteringConfiguration(MeteringMethod meteringMethod, string meterNumber)
        {
            Assert.Equal(meteringMethod, EnumerationType.FromName<MeteringMethod>(_meteringPoint.MeteringPointSubType));
            Assert.Equal(meterNumber, _meteringPoint.MeterNumber);
            return this;
        }

        public AssertPersistedMeteringPoint HasSettlementMethod(SettlementMethod settlementMethod)
        {
            Assert.Equal(settlementMethod, EnumerationType.FromName<SettlementMethod>(_meteringPoint.SettlementMethod));
            return this;
        }

        public AssertPersistedMeteringPoint HasScheduledMeterReadingDate(string scheduledMeterReadingDate)
        {
            Assert.Equal(scheduledMeterReadingDate, _meteringPoint.ScheduledMeterReadingDate);
            return this;
        }

        public AssertPersistedMeteringPoint HasConnectionType(ConnectionType connectionType)
        {
            Assert.Equal(connectionType, EnumerationType.FromName<ConnectionType>(_meteringPoint.ConnectionType));
            return this;
        }

        public AssertPersistedMeteringPoint HasDisconnectionType(DisconnectionType disconnectionType)
        {
            Assert.Equal(disconnectionType, EnumerationType.FromName<DisconnectionType>(_meteringPoint.DisconnectionType));
            return this;
        }

        public AssertPersistedMeteringPoint HasNetSettlementGroup(NetSettlementGroup netSettlementGroup)
        {
            Assert.Equal(netSettlementGroup, EnumerationType.FromName<NetSettlementGroup>(_meteringPoint.NetSettlementGroup));
            return this;
        }

        public AssertPersistedMeteringPoint HasProductionObligation(bool productionObligation)
        {
            Assert.Equal(productionObligation, _meteringPoint.ProductionObligation);
            return this;
        }

        public AssertPersistedMeteringPoint HasParentMeteringPoint(string? gsrnNumber)
        {
            var parent = _connectionFactory.GetOpenConnection().QuerySingleOrDefault(
                $"SELECT pmp.GsrnNumber" +
                $" FROM [dbo].[MeteringPoints] mp" +
                $" JOIN [dbo].[MeteringPoints] pmp ON pmp.Id = mp.ParentRelatedMeteringPoint" +
                $" WHERE mp.GsrnNumber = {_meteringPoint.GsrnNumber}");

            Assert.Equal(gsrnNumber, parent?.GsrnNumber);
            return this;
        }
    }
}
