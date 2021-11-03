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

using System;
using Energinet.DataHub.MeteringPoints.Domain.Addresses;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public record MasterDataDetails(
        string? StreetName = null,
        string? PostCode = null,
        string? City = null,
        string? StreetCode = null,
        string? BuildingNumber = null,
        string? CitySubDivision = null,
        CountryCode? CountryCode = null,
        string? Floor = null,
        string? Room = null,
        int? MunicipalityCode = null,
        bool? IsActual = null,
        Guid? GeoInfoReference = null);
}
