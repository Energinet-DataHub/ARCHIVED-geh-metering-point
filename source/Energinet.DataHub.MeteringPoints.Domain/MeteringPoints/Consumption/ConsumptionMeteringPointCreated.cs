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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption
{
    public class ConsumptionMeteringPointCreated : DomainEventBase
    {
        public ConsumptionMeteringPointCreated(
            string meteringPointType,
            Guid meteringPointId,
            string gsrnNumber,
            Guid gridAreaId,
            string meteringPointSubType,
            string productType,
            string readingOccurrence,
            string unitType,
            string settlementMethod,
            string netSettlementGroup,
            string? city,
            string? floor,
            string? room,
            string? buildingNumber,
            string? countryCode,
            int? municipalityCode,
            string? postCode,
            string? streetCode,
            string? streetName,
            string? citySubDivision,
            bool isActualAddress,
            Guid? geoInfoReference,
            string? powerPlantGsrnNumber,
            string? locationDescription,
            string? meterNumber,
            int maximumCurrent,
            int maximumPower,
            Instant effectiveDate,
            string disconnectionType,
            string? connectionType,
            string? assetType,
            string physicalState,
            string? scheduledMeterReadingDate,
            double? capacity,
            Guid? sourceGridAreaLinkId,
            Guid? targetGridAreaLinkId)
        {
            MeteringPointType = meteringPointType;
            MeteringPointId = meteringPointId;
            GsrnNumber = gsrnNumber;
            GridAreaLinkId = gridAreaId;
            MeteringPointSubType = meteringPointSubType;
            ProductType = productType;
            ReadingOccurrence = readingOccurrence;
            UnitType = unitType;
            SettlementMethod = settlementMethod;
            NetSettlementGroup = netSettlementGroup;
            City = city;
            Floor = floor;
            Room = room;
            BuildingNumber = buildingNumber;
            CountryCode = countryCode;
            MunicipalityCode = municipalityCode;
            PostCode = postCode;
            StreetCode = streetCode;
            StreetName = streetName;
            CitySubDivision = citySubDivision;
            IsActualAddress = isActualAddress;
            GeoInfoReference = geoInfoReference;
            PowerPlantGsrnNumber = powerPlantGsrnNumber;
            LocationDescription = locationDescription;
            MeterNumber = meterNumber;
            MaximumCurrent = maximumCurrent;
            MaximumPower = maximumPower;
            EffectiveDate = effectiveDate;
            DisconnectionType = disconnectionType;
            ConnectionType = connectionType;
            AssetType = assetType;
            PhysicalState = physicalState;
            ScheduledMeterReadingDate = scheduledMeterReadingDate;
            Capacity = capacity;
            SourceGridAreaLinkId = sourceGridAreaLinkId;
            TargetGridAreaLinkId = targetGridAreaLinkId;
        }

        public string MeteringPointType { get; }

        public Guid MeteringPointId { get; }

        public string GsrnNumber { get; }

        public Guid GridAreaLinkId { get; }

        public string MeteringPointSubType { get; }

        public string ProductType { get; }

        public string ReadingOccurrence { get; }

        public string UnitType { get; }

        public string SettlementMethod { get; }

        public string NetSettlementGroup { get; }

        public string? City { get; }

        public string? Floor { get; }

        public string? Room { get; }

        public string? BuildingNumber { get; }

        public string? CountryCode { get; }

        public int? MunicipalityCode { get; }

        public string? PostCode { get; }

        public string? StreetCode { get; }

        public string? StreetName { get; }

        public string? CitySubDivision { get; }

        public bool IsActualAddress { get; }

        public string? PowerPlantGsrnNumber { get; }

        public string? LocationDescription { get; }

        public string? MeterNumber { get; }

        public int MaximumCurrent { get; }

        public int MaximumPower { get; }

        public Instant EffectiveDate { get; }

        public string DisconnectionType { get; }

        public string? ConnectionType { get; }

        public string? AssetType { get; }

        public string PhysicalState { get; }

        public string? ScheduledMeterReadingDate { get; }

        public Guid? GeoInfoReference { get; }

        public double? Capacity { get; }

        public Guid? SourceGridAreaLinkId { get; }

        public Guid? TargetGridAreaLinkId { get; }
    }
}
