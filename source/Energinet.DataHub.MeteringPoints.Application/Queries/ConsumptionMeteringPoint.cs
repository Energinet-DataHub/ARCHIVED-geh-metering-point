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
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Application.Queries
{
    public record ConsumptionMeteringPoint
    {
        public ConsumptionMeteringPoint()
        {
            Id = default!;
            GsrnNumber = default!;
            StreetName = default!;
            PostCode = default!;
            CityName = default!;
            CountryCode = default!;
            PhysicalState = default!;
            MeteringPointSubType = default!;
            MeteringPointType = default!;
            GridAreaId = default!;
            PowerPlantGsrnNumber = default!;
            LocationDescription = default!;
            UnitType = default!;
            MeterNumber = default!;
            MeterReadingOccurrence = default!;
            MaximumCurrent = default!;
            MaximumPower = default!;
            SettlementMethod = default!;
            NetSettlementGroup = default!;
            DisconnectionType = default!;
            ConnectionType = default!;
            AssetType = default!;
            ParentRelatedMeteringPoint = default!;
            ProductType = default!;
        }

        public ConsumptionMeteringPoint(
            Guid id,
            string gsrnNumber,
            string streetName,
            string postCode,
            string cityName,
            string countryCode,
            string physicalState,
            string meteringPointSubType,
            string meteringPointType,
            string gridAreaId,
            string? powerPlantGsrnNumber,
            string? locationDescription,
            string unitType,
            string meterNumber,
            string meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            string settlementMethod,
            string netSettlementGroup,
            string disconnectionType,
            string connectionType,
            string? assetType,
            string? parentRelatedMeteringPoint,
            string productType)
        {
            Id = id;
            GsrnNumber = gsrnNumber;
            StreetName = streetName;
            PostCode = postCode;
            CityName = cityName;
            CountryCode = countryCode;
            PhysicalState = physicalState;
            MeteringPointSubType = meteringPointSubType;
            MeteringPointType = meteringPointType;
            GridAreaId = gridAreaId;
            PowerPlantGsrnNumber = powerPlantGsrnNumber;
            LocationDescription = locationDescription;
            UnitType = unitType;
            MeterNumber = meterNumber;
            MeterReadingOccurrence = meterReadingOccurrence;
            MaximumCurrent = maximumCurrent;
            MaximumPower = maximumPower;
            SettlementMethod = settlementMethod;
            NetSettlementGroup = netSettlementGroup;
            DisconnectionType = disconnectionType;
            ConnectionType = connectionType;
            AssetType = assetType;
            ParentRelatedMeteringPoint = parentRelatedMeteringPoint;
            ProductType = productType;
        }

        public Guid Id { get; set; }

        public string GsrnNumber { get; set; }

        public string StreetName { get; set; }

        public string PostCode { get; set; }

        public string CityName { get; set; }

        public string CountryCode { get; set; }

        public string PhysicalState { get; set; }

        public string MeteringPointSubType { get; set; }

        public string MeteringPointType { get; set; }

        public string GridAreaId { get; set; }

        public string? PowerPlantGsrnNumber { get; set; }

        public string? LocationDescription { get; set; }

        public string UnitType { get; set; }

        public string MeterNumber { get; set; }

        public string MeterReadingOccurrence { get; set; }

        public int MaximumCurrent { get; set; }

        public int MaximumPower { get; set; }

        public string SettlementMethod { get; set; }

        public string NetSettlementGroup { get; set; }

        public string DisconnectionType { get; set; }

        public string ConnectionType { get; set; }

        public string? AssetType { get; set; }

        public string? ParentRelatedMeteringPoint { get; set; }

        public string ProductType { get; set; }
    }
}
