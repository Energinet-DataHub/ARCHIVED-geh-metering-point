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
using System.ComponentModel.DataAnnotations;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums;

namespace Energinet.DataHub.MeteringPoints.Client.Abstractions.Models
{
    public class MeteringPointCimDto
    {
        [Required]
        public Guid MeteringPointId { get; set; }

        [Required]
        public string GsrnNumber { get; set; } = null!;

        [Required]
        public string StreetName { get; set; } = null!;

        [Required]
        public string PostalCode { get; set; } = null!;

        [Required]
        public string CityName { get; set; } = null!;

        [Required]
        public string CountryCode { get; set; } = null!;

        [Required]
        public ConnectionState ConnectionState { get; set; }

        [Required]
        public MeteringMethod MeteringMethod { get; set; }

        [Required]
        public ReadingOccurrence ReadingOccurrence { get; set; }

        [Required]
        public MeteringPointType MeteringPointType { get; set; }

        public int? RatedCapacity { get; set; }

        public int? RatedCurrent { get; set; }

        [Required]
        public string GridAreaName { get; set; } = null!;

        [Required]
        public string GridAreaCode { get; set; } = null!;

        public string? LinkedExtendedMasterdata { get; set; }

        public string? LocationDescription { get; set; }

        [Required]
        public ProductId ProductId { get; set; }

        [Required]
        public Unit Unit { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        public string? MeterId { get; set; }

        public string? StreetCode { get; set; }

        public string? CitySubDivisionName { get; set; }

        public string? FloorIdentification { get; set; }

        public string? SuiteNumber { get; set; }

        public string? BuildingNumber { get; set; }

        public int? MunicipalityCode { get; set; }

        public bool? IsActualAddress { get; set; }

        public Guid? DarReference { get; set; }

        public double? Capacity { get; set; }

        public AssetType? AssetType { get; set; }

        public SettlementMethod? SettlementMethod { get; set; }

        public string? InAreaCode { get; set; }

        public string? OutAreaCode { get; set; }

        public NetSettlementGroup? NetSettlementGroup { get; set; }

        public DateTime? SupplyStart { get; set; }

        public ConnectionType? ConnectionType { get; set; }

        public DisconnectionType? DisconnectionType { get; set; }

        public bool? ProductionObligation { get; set; }

        public IEnumerable<MeteringPointSimpleCimDto>? ChildMeteringPoints { get; set; }

        public MeteringPointSimpleCimDto? ParentMeteringPoint { get; set; }

        public string? PowerPlantGsrnNumber { get; set; }
    }
}
