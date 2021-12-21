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
using System.Collections.ObjectModel;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Exceptions;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses
{
    public class Address : ValueObject
    {
        private Address(string? streetName, string? streetCode, string? buildingNumber, string? city, string? citySubDivision, string? postCode, CountryCode? countryCode, string? floor, string? room, int? municipalityCode, bool? isActual, Guid? geoInfoReference, string? locationDescription)
        {
            StreetName = streetName;
            StreetCode = streetCode;
            BuildingNumber = buildingNumber;
            PostCode = postCode;
            City = city;
            CitySubDivision = citySubDivision;
            CountryCode = countryCode;
            Floor = floor;
            Room = room;
            MunicipalityCode = municipalityCode;
            IsActual = isActual;
            GeoInfoReference = geoInfoReference;
            LocationDescription = locationDescription;
        }

        public string? StreetName { get; }

        public string? StreetCode { get; }

        public string? BuildingNumber { get; }

        public string? PostCode { get; }

        public string? City { get; }

        public string? CitySubDivision { get; }

        public CountryCode? CountryCode { get; }

        public string? Floor { get; }

        public string? Room { get; }

        public int? MunicipalityCode { get; }

        public bool? IsActual { get; }

        public Guid? GeoInfoReference { get; }

        public string? LocationDescription { get; }

        public static Address Create(
            string? streetName = null,
            string? streetCode = null,
            string? buildingNumber = null,
            string? city = null,
            string? citySubDivision = null,
            string? postCode = null,
            CountryCode? countryCode = null,
            string? floor = null,
            string? room = null,
            int? municipalityCode = null,
            bool? isActual = null,
            Guid? geoInfoReference = null,
            string? locationDescription = null)
        {
            var result = CheckRules(streetName, streetCode, buildingNumber, city, citySubDivision, postCode, countryCode, floor, room, municipalityCode, locationDescription);
            if (result.Success == false)
            {
                throw new InvalidAddressException(result.Errors);
            }

            return new(
                streetName: streetName,
                streetCode: streetCode,
                buildingNumber: buildingNumber,
                city: city,
                citySubDivision: citySubDivision,
                postCode: postCode,
                countryCode: countryCode,
                floor: floor,
                room: room,
                municipalityCode: municipalityCode,
                isActual: isActual,
                geoInfoReference: geoInfoReference,
                locationDescription: locationDescription);
        }

        public static BusinessRulesValidationResult CheckRules(string? streetName, string? streetCode, string? buildingNumber, string? city, string? citySubDivision, string? postCode, CountryCode? countryCode, string? floor, string? room, int? municipalityCode, string? locationDescription)
        {
            return new(new Collection<IBusinessRule>()
            {
                new StreetNameLengthRule(streetName),
                new StreetCodeLengthRule(streetCode),
                new BuildingNumberFormatRule(buildingNumber, countryCode),
                new FloorLengthRule(floor),
                new RoomLengthRule(room),
                new CityNameLengthRule(city),
                new PostCodeFormatRule(postCode, countryCode),
                new CitySubdivisionRule(citySubDivision),
                new MunicipalityCodeRule(municipalityCode),
                new LocationDescriptionLengthRule(locationDescription),
            });
        }

        public Address MergeFrom(Address address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            return Create(
                address.StreetName ?? StreetName,
                address.StreetCode ?? StreetCode,
                address.BuildingNumber ?? BuildingNumber,
                address.City ?? City,
                address.CitySubDivision ?? CitySubDivision,
                address.PostCode ?? PostCode,
                address.CountryCode ?? CountryCode,
                address.Floor ?? Floor,
                address.Room ?? Room,
                address.MunicipalityCode ?? MunicipalityCode,
                address.IsActual ?? IsActual,
                address.GeoInfoReference ?? GeoInfoReference,
                address.LocationDescription ?? LocationDescription);
        }
    }
}
