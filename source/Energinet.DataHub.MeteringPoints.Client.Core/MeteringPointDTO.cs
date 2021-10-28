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

namespace Energinet.DataHub.MeteringPoints.Client.Core
{
    public class MeteringPointDTO
    {
#pragma warning disable 8618 // Remove parameterless constructor when all fields are included and named properly in MeteringPointByGsrnQueryHandler
        public MeteringPointDTO() { }
#pragma warning restore 8618

        public MeteringPointDTO(
            Guid meteringPointId,
            string gsrnNumber,
            string settlementMethod,
            string connectionState,
            string meteringPointType,
            string meteringPointSubType,
            string readingOccurrence,
            string netSettlementGroup,
            string supplyStart,
            int maximumCurrent,
            int maximumPower,
            string gridAreaName,
            string gridAreaCode,
            string fromGridAreaCode,
            string toGridAreaCode,
            bool productionObligation,
            string powerPlantGsrnNumber,
            string locationDescription,
            string product,
            string unitType,
            string disconnectionType,
            string connectionType,
            string capacity,
            string assetType,
            string effectiveDate,
            string streetName,
            string streetCode,
            string buildingNumber,
            string floor,
            string suite,
            string citySubdivisionName,
            string postCode,
            string cityName,
            string municipalityCode,
            string countryCode,
            bool isActualAddress,
            Guid geoInfoReference,
            string meterNumber)
        {
            MeteringPointId = meteringPointId;
            GsrnNumber = gsrnNumber;
            SettlementMethod = settlementMethod;
            ConnectionState = connectionState;
            MeteringPointType = meteringPointType;
            MeteringPointSubType = meteringPointSubType;
            ReadingOccurrence = readingOccurrence;
            NetSettlementGroup = netSettlementGroup;
            SupplyStart = supplyStart;
            MaximumCurrent = maximumCurrent;
            MaximumPower = maximumPower;
            GridAreaName = gridAreaName;
            GridAreaCode = gridAreaCode;
            FromGridAreaCode = fromGridAreaCode;
            ToGridAreaCode = toGridAreaCode;
            ProductionObligation = productionObligation;
            PowerPlantGsrnNumber = powerPlantGsrnNumber;
            LocationDescription = locationDescription;
            Product = product;
            UnitType = unitType;
            DisconnectionType = disconnectionType;
            ConnectionType = connectionType;
            Capacity = capacity;
            AssetType = assetType;
            EffectiveDate = effectiveDate;
            StreetName = streetName;
            StreetCode = streetCode;
            BuildingNumber = buildingNumber;
            Floor = floor;
            Suite = suite;
            CitySubdivisionName = citySubdivisionName;
            PostCode = postCode;
            CityName = cityName;
            MunicipalityCode = municipalityCode;
            CountryCode = countryCode;
            IsActualAddress = isActualAddress;
            GeoInfoReference = geoInfoReference;
            MeterNumber = meterNumber;
        }

        public Guid MeteringPointId { get; }

        public string GsrnNumber { get; }

        // Afregningsform
        public string SettlementMethod { get; }

        // Tilslutningsstatus
        public string ConnectionState { get; }

        // Målepunktstype
        public string MeteringPointType { get; }

        // Målepunkts art
        public string MeteringPointSubType { get; }

        // Aflæsningsfrekvens
        public string ReadingOccurrence { get; }

        // Nettoafregningsgruppe
        public string NetSettlementGroup { get; }

        // Start af leverance
        public string SupplyStart { get; }

        // Effektgrænse Ampere
        public int MaximumCurrent { get; }

        // Effektgrænse kW
        public int MaximumPower { get; }

        // Netområde navn
        public string GridAreaName { get; }

        // Netområde kode
        public string GridAreaCode { get; }

        // Fra net kode
        public string FromGridAreaCode { get; }

        // Til net kode
        public string ToGridAreaCode { get; }

        // Aftagepligt
        public bool ProductionObligation { get; }

        // VærksGSRN
        public string PowerPlantGsrnNumber { get; }

        // Målepunktskommentar
        public string LocationDescription { get; }

        // Produkt
        public string Product { get; }

        // Energienhed
        public string UnitType { get; }

        // Afbrydelsesart
        public string DisconnectionType { get; }

        // Tilslutningstype
        public string ConnectionType { get; }

        // Anlægskapacitet
        public string Capacity { get; }

        // Anlægsteknologi
        public string AssetType { get; }

        // Gyldighedsdato
        public string EffectiveDate { get; }

        // Vejnavn
        public string StreetName { get; }

        // Vejkode
        public string StreetCode { get; }

        // Husnummer
        public string BuildingNumber { get; }

        // Etage
        public string Floor { get; }

        // Dør
        public string Suite { get; }

        // Supplerende bynavn
        public string CitySubdivisionName { get; }

        // Postnummer
        public string PostCode { get; }

        // Postdistrikt
        public string CityName { get; }

        // Kommunekode
        public string MunicipalityCode { get; }

        // Landekode
        public string CountryCode { get; }

        // Vaskeanvisning
        public bool IsActualAddress { get; }

        // DAR Reference
        public Guid GeoInfoReference { get; }

        // Målernummer
        public string MeterNumber { get; }
    }
}
