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

namespace Energinet.DataHub.MeteringPoints.Client.Abstractions.Models
{
    public record MeteringPointDto(
        Guid MeteringPointId,
        string GsrnNumber,
        string StreetName,
        string PostCode,
        string CityName,
        string CountryCode,
        string ConnectionState,
        string MeteringPointSubType,
        string ReadingOccurrence,
        string MeteringPointType,
        int? MaximumCurrent,
        int? MaximumPower,
        string GridAreaName,
        string GridAreaCode,
        string PowerPlantGsrnNumber,
        string LocationDescription,
        string Product,
        string UnitType,
        DateTime? EffectiveDate,
        string MeterNumber,
        string StreetCode,
        string CitySubDivisionName,
        string Floor,
        string Suite,
        string BuildingNumber,
        int? MunicipalityCode,
        bool? IsActualAddress,
        Guid? GeoInfoReference,
        double? Capacity,
        string SettlementMethod,
        string NetSettlementGroup,
        string AssetType,
        string? ToGridAreaCode,
        string? FromGridAreaCode,
        string ConnectionType,
        string DisconnectionType,
        bool? ProductionObligation);
}
