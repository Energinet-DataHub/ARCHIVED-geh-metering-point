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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Events
{
    public class MasterDataWasUpdated : DomainEventBase
    {
        public MasterDataWasUpdated(string? streetName, string? streetCode, string? city, string? floor, string? room, string? buildingNumber, string? countryCode, bool? isActual, string? locationDescription, int? municipalityCode, string? postCode, string? citySubDivision, Guid? geoInfoReference, double? capacity, string? assetType, string? connectionType, string? disconnectionType, string? effectiveDate, string? meterNumber, string? meteringMethod, int? powerLimitInAmpere, int? powerLimitInKwh, bool? productionObligation, string? productType, string? readingOccurrence, string? settlementMethod, string? unitType, string? netSettlementGroup, string? powerPlantGsrnNumber, string? scheduledMeterReadingDate)
        {
            StreetName = streetName;
            StreetCode = streetCode;
            City = city;
            Floor = floor;
            Room = room;
            BuildingNumber = buildingNumber;
            CountryCode = countryCode;
            IsActual = isActual;
            LocationDescription = locationDescription;
            MunicipalityCode = municipalityCode;
            PostCode = postCode;
            CitySubDivision = citySubDivision;
            GeoInfoReference = geoInfoReference;
            Capacity = capacity;
            AssetType = assetType;
            ConnectionType = connectionType;
            DisconnectionType = disconnectionType;
            EffectiveDate = effectiveDate;
            MeterNumber = meterNumber;
            MeteringMethod = meteringMethod;
            PowerLimitInAmpere = powerLimitInAmpere;
            PowerLimitInKwh = powerLimitInKwh;
            ProductionObligation = productionObligation;
            ProductType = productType;
            ReadingOccurrence = readingOccurrence;
            SettlementMethod = settlementMethod;
            UnitType = unitType;
            NetSettlementGroup = netSettlementGroup;
            PowerPlantGsrnNumber = powerPlantGsrnNumber;
            ScheduledMeterReadingDate = scheduledMeterReadingDate;
        }

        public string? StreetName { get; }

        public string? StreetCode { get; }

        public string? City { get; }

        public string? Floor { get; }

        public string? Room { get; }

        public string? BuildingNumber { get; }

        public string? CountryCode { get; }

        public bool? IsActual { get; }

        public string? LocationDescription { get; }

        public int? MunicipalityCode { get; }

        public string? PostCode { get; }

        public string? CitySubDivision { get; }

        public Guid? GeoInfoReference { get; }

        public double? Capacity { get; }

        public string? AssetType { get; }

        public string? ConnectionType { get; }

        public string? DisconnectionType { get; }

        public string? EffectiveDate { get; }

        public string? MeterNumber { get; }

        public string? MeteringMethod { get; }

        public int? PowerLimitInAmpere { get; }

        public int? PowerLimitInKwh { get; }

        public bool? ProductionObligation { get; }

        public string? ProductType { get; }

        public string? ReadingOccurrence { get; }

        public string? SettlementMethod { get; }

        public string? UnitType { get; }

        public string? NetSettlementGroup { get; }

        public string? PowerPlantGsrnNumber { get; }

        public string? ScheduledMeterReadingDate { get; }
    }
}
