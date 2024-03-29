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
using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums;

namespace Energinet.DataHub.MeteringPoints.Client.Abstractions.Models
{
    public record MeteringPointCimDto(
        Guid MeteringPointId,
        string GsrnNumber,
        string StreetName,
        string PostalCode,
        string CityName,
        string CountryCode,
        ConnectionState ConnectionState,
        MeteringMethod MeteringMethod,
        ReadingOccurrence ReadingOccurrence,
        MeteringPointType MeteringPointType,
        int? RatedCapacity,
        int? RatedCurrent,
        string GridAreaName,
        string GridAreaCode,
        string? LinkedExtendedMasterdata,
        string? LocationDescription,
        ProductId ProductId,
        Unit Unit,
        DateTime EffectiveDate,
        string? MeterId,
        string? StreetCode,
        string? CitySubDivisionName,
        string? FloorIdentification,
        string? SuiteNumber,
        string? BuildingNumber,
        int? MunicipalityCode,
        bool? IsActualAddress,
        Guid? DarReference,
        double? Capacity,
        AssetType? AssetType,
        SettlementMethod? SettlementMethod,
        string? InAreaCode,
        string? OutAreaCode,
        NetSettlementGroup? NetSettlementGroup,
        DateTime? SupplyStart,
        ConnectionType? ConnectionType,
        DisconnectionType? DisconnectionType,
        bool? ProductionObligation,
        IEnumerable<MeteringPointSimpleCimDto>? ChildMeteringPoints,
        MeteringPointSimpleCimDto? ParentMeteringPoint,
        string? PowerPlantGsrnNumber);
}
