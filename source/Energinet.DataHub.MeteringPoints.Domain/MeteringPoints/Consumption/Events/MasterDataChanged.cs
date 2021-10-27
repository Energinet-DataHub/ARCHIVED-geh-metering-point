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
using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Events
{
    public class MasterDataChanged : DomainEventBase
    {
        public MasterDataChanged(string? streetName, string? postCode, string? city, string? streetCode, string? buildingNumber, string? citySubDivision, string? countryCode, string? floor, string? room, int municipalityCode, bool isActual, Guid geoInfoReference)
        {
            StreetName = streetName;
            PostCode = postCode;
            City = city;
            StreetCode = streetCode;
            BuildingNumber = buildingNumber;
            CitySubDivision = citySubDivision;
            CountryCode = countryCode;
            Floor = floor;
            Room = room;
            MunicipalityCode = municipalityCode;
            IsActual = isActual;
            GeoInfoReference = geoInfoReference;
        }

        public string? StreetName { get; }

        public string? PostCode { get; }

        public string? City { get; }

        public string? StreetCode { get; }

        public string? BuildingNumber { get; }

        public string? CitySubDivision { get; }

        public string? CountryCode { get; }

        public string? Floor { get; }

        public string? Room { get; }

        public int MunicipalityCode { get; }

        public bool IsActual { get; }

        public Guid GeoInfoReference { get; }
    }
}
