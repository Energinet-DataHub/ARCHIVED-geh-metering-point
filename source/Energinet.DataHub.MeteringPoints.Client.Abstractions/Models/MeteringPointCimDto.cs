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
            IEnumerable<MeteringPointSimpleCimDto> childMeteringPoints,
            MeteringPointSimpleCimDto parentMeteringPoint,
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
        public Guid MeteringPointId { get; private set; }

        [Required]
        public string GsrnNumber { get; private set; } = null!;

        [Required]
        public string StreetName { get; private set; } = null!;

        [Required]
        public string PostalCode { get; private set; } = null!;

        [Required]
        public string CityName { get; private set; } = null!;

        [Required]
        public string CountryCode { get; private set; } = null!;

        [Required]
        public ConnectionState ConnectionState { get; private set; }

        [Required]
        public MeteringMethod MeteringMethod { get; private set; }

        [Required]
        public ReadingOccurrence ReadingOccurrence { get; private set; }

        [Required]
        public MeteringPointType MeteringPointType { get; private set; }

        public int? RatedCapacity { get; private set; }

        public int? RatedCurrent { get; private set; }

        [Required]
        public string GridAreaName { get; private set; } = null!;

        [Required]
        public string GridAreaCode { get; private set; } = null!;

        public string? LinkedExtendedMasterdata { get; private set; }

        public string? LocationDescription { get; private set; }

        [Required]
        public ProductId ProductId { get; private set; }

        [Required]
        public Unit Unit { get; private set; }

        [Required]
        public DateTime EffectiveDate { get; private set; }

        public string? MeterId { get; private set; }

        public string? StreetCode { get; private set; }

        public string? CitySubDivisionName { get; private set; }

        public string? FloorIdentification { get; private set; }

        public string? SuiteNumber { get; private set; }

        public string? BuildingNumber { get; private set; }

        public int? MunicipalityCode { get; private set; }

        public bool? IsActualAddress { get; private set; }

        public Guid? DarReference { get; private set; }

        public double? Capacity { get; private set; }

        public AssetType? AssetType { get; private set; }

        public SettlementMethod? SettlementMethod { get; private set; }

        public string? InAreaCode { get; private set; }

        public string? OutAreaCode { get; private set; }

        public NetSettlementGroup? NetSettlementGroup { get; private set; }

        public DateTime? SupplyStart { get; private set; }

        public ConnectionType? ConnectionType { get; private set; }

        public DisconnectionType? DisconnectionType { get; private set; }

        public bool? ProductionObligation { get; private set; }

        public IEnumerable<MeteringPointSimpleCimDto>? ChildMeteringPoints { get; private set; }

        public MeteringPointSimpleCimDto? ParentMeteringPoint { get; private set; }

        public string? PowerPlantGsrnNumber { get; private set; }
    }
}
