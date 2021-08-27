﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public class Address : ValueObject
    {
        private Address(string? streetName, string? streetCode, string? postCode, string? cityName, string? countryCode, string? floor, string? room)
        {
            StreetName = streetName;
            StreetCode = streetCode;
            PostCode = postCode;
            CityName = cityName;
            CountryCode = countryCode;
            Floor = floor;
            Room = room;
        }

        public string? StreetName { get; }

        public string? StreetCode { get; }

        public string? PostCode { get; }

        public string? CityName { get; }

        public string? CountryCode { get; }

        public string? Floor { get; }

        public string? Room { get; }

        public static Address Create(string? streetName, string? streetCode, string? buildingNumber, string? postCode, string? cityName, string? countryCode, string? floor, string? room)
        {
            if (CheckRules(streetName, streetCode, buildingNumber, countryCode, floor, room).Success == false)
            {
                throw new InvalidAddressException();
            }

            return new(streetName, streetCode, postCode, cityName, countryCode, floor, room);
        }

        public static BusinessRulesValidationResult CheckRules(string? streetName, string? streetCode, string? buildingNumber, string? countryCode, string? floor, string? room)
        {
            return new BusinessRulesValidationResult(new Collection<IBusinessRule>()
            {
                new StreetNameLengthRule(streetName),
                new StreetCodeLengthRule(streetCode),
                new BuildingNumberFormatRule(buildingNumber, countryCode),
                new FloorLengthRule(floor),
                new RoomLengthRule(room),
            });
        }
    }
}
