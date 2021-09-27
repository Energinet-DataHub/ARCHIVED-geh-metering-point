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

using System.Collections.ObjectModel;
using Energinet.DataHub.MeteringPoints.Domain.Addresses.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.Addresses
{
    public class Address : ValueObject
    {
        private Address(string? streetName, string? streetCode, string? buildingNumber, string? city, string? citySubDivision, string? postCode, CountryCode? countryCode, string? floor, string? room, int? municipalityCode, bool isOfficial)
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
            IsOfficial = isOfficial;
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

        public bool IsOfficial { get; }

        public static Address Create(string? streetName, string? streetCode, string? buildingNumber, string? city, string? citySubDivision, string? postCode, CountryCode? countryCode, string? floor, string? room, int? municipalityCode, bool isOfficial)
        {
            if (CheckRules(streetName, streetCode, buildingNumber, city, citySubDivision, postCode, countryCode, floor, room, municipalityCode).Success == false)
            {
                throw new InvalidAddressException();
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
                isOfficial: isOfficial);
        }

        public static BusinessRulesValidationResult CheckRules(string? streetName, string? streetCode, string? buildingNumber, string? city, string? citySubDivision, string? postCode, CountryCode? countryCode, string? floor, string? room, int? municipalityCode)
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
            });
        }
    }
}
