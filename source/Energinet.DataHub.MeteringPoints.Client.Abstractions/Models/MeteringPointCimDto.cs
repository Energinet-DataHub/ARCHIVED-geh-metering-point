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
        public MeteringPointCimDto(
            Guid meteringPointId,
            string gsrnNumber,
            string streetName,
            string postalCode,
            string cityName,
            string countryCode,
            ConnectionState connectionState,
            MeteringMethod meteringMethod,
            ReadingOccurrence readingOccurence,
            MeteringPointType meteringPointType,
            int? ratedCapacity,
            int? ratedCurrent,
            string gridAreaName,
            string gridAreaCode,
            string? linkedExtendedMasterdata,
            string? locationDescription,
            ProductId productId,
            Unit unit,
            DateTime effectiveDate,
            string? meterId,
            string? streetCode,
            string? citySubDivisionName,
            string? floorIdentification,
            string? suiteNumber,
            string? buildingNumber,
            int? municipalityCode,
            bool? isActualAddress,
            Guid? darReference,
            double? capacity,
            AssetType? assetType,
            SettlementMethod? settlementMethod,
            string? inAreaCode,
            string? outAreaCode,
            NetSettlementGroup? netSettlementGroup,
            DateTime? supplyStart,
            ConnectionType? connectionType,
            DisconnectionType? disconnectionType,
            bool? productionObligation,
            IEnumerable<MeteringPointSimpleCimDto>? childMeteringPoints,
            MeteringPointSimpleCimDto? parentMeteringPoint,
            string? powerPlantGsrnNumber)
        {
            MeteringPointId = meteringPointId;
            GsrnNumber = gsrnNumber;
            StreetName = streetName;
            PostalCode = postalCode;
            CityName = cityName;
            CountryCode = countryCode;
            ConnectionState = connectionState;
            MeteringMethod = meteringMethod;
            ReadingOccurrence = readingOccurence;
            MeteringPointType = meteringPointType;
            RatedCapacity = ratedCapacity;
            RatedCurrent = ratedCurrent;
            GridAreaName = gridAreaName;
            GridAreaCode = gridAreaCode;
            LinkedExtendedMasterdata = linkedExtendedMasterdata;
            LocationDescription = locationDescription;
            ProductId = productId;
            Unit = unit;
            EffectiveDate = effectiveDate;
            MeterId = meterId;
            StreetCode = streetCode;
            CitySubDivisionName = citySubDivisionName;
            FloorIdentification = floorIdentification;
            SuiteNumber = suiteNumber;
            BuildingNumber = buildingNumber;
            MunicipalityCode = municipalityCode;
            IsActualAddress = isActualAddress;
            DarReference = darReference;
            Capacity = capacity;
            AssetType = assetType;
            SettlementMethod = settlementMethod;
            InAreaCode = inAreaCode;
            OutAreaCode = outAreaCode;
            NetSettlementGroup = netSettlementGroup;
            SupplyStart = supplyStart;
            ConnectionType = connectionType;
            DisconnectionType = disconnectionType;
            ProductionObligation = productionObligation;
            ChildMeteringPoints = childMeteringPoints;
            ParentMeteringPoint = parentMeteringPoint;
            PowerPlantGsrnNumber = powerPlantGsrnNumber;
        }

        [Required]
        public Guid MeteringPointId { get; }

        [Required]
        public string GsrnNumber { get; } = null!;

        [Required]
        public string StreetName { get; } = null!;

        [Required]
        public string PostalCode { get; } = null!;

        [Required]
        public string CityName { get; } = null!;

        [Required]
        public string CountryCode { get; } = null!;

        [Required]
        public ConnectionState ConnectionState { get; }

        [Required]
        public MeteringMethod MeteringMethod { get; }

        [Required]
        public ReadingOccurrence ReadingOccurrence { get; }

        [Required]
        public MeteringPointType MeteringPointType { get; }

        public int? RatedCapacity { get; }

        public int? RatedCurrent { get; }

        [Required]
        public string GridAreaName { get; } = null!;

        [Required]
        public string GridAreaCode { get; } = null!;

        public string? LinkedExtendedMasterdata { get; }

        public string? LocationDescription { get; }

        [Required]
        public ProductId ProductId { get; }

        [Required]
        public Unit Unit { get; }

        [Required]
        public DateTime EffectiveDate { get; }

        public string? MeterId { get; }

        public string? StreetCode { get; }

        public string? CitySubDivisionName { get; }

        public string? FloorIdentification { get; }

        public string? SuiteNumber { get; }

        public string? BuildingNumber { get; }

        public int? MunicipalityCode { get; }

        public bool? IsActualAddress { get; }

        public Guid? DarReference { get; }

        public double? Capacity { get; }

        public AssetType? AssetType { get; }

        public SettlementMethod? SettlementMethod { get; }

        public string? InAreaCode { get; }

        public string? OutAreaCode { get; }

        public NetSettlementGroup? NetSettlementGroup { get; }

        public DateTime? SupplyStart { get; }

        public ConnectionType? ConnectionType { get; }

        public DisconnectionType? DisconnectionType { get; }

        public bool? ProductionObligation { get; }

        public IEnumerable<MeteringPointSimpleCimDto>? ChildMeteringPoints { get; }

        public MeteringPointSimpleCimDto? ParentMeteringPoint { get; }

        public string? PowerPlantGsrnNumber { get; }
    }
}
