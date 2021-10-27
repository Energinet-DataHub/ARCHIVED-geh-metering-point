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

namespace Energinet.DataHub.MeteringPoints.Application.Queries
{
    public record ProductionMeteringPoint
    {
        public ProductionMeteringPoint()
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
            ProductionObligation = default!;
            NetSettlementGroup = default!;
            DisconnectionType = default!;
            ConnectionType = default!;
            AssetType = default!;
            ParentRelatedMeteringPoint = default!;
            ProductType = default!;
        }

        public ProductionMeteringPoint(
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
            bool productionObligation,
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
            ProductionObligation = productionObligation;
            NetSettlementGroup = netSettlementGroup;
            DisconnectionType = disconnectionType;
            ConnectionType = connectionType;
            AssetType = assetType;
            ParentRelatedMeteringPoint = parentRelatedMeteringPoint;
            ProductType = productType;
        }

        public Guid Id { get; private set; }

        public string GsrnNumber { get; private set; }

        public string StreetName { get; private set; }

        public string PostCode { get; private set; }

        public string CityName { get; private set; }

        public string CountryCode { get; private set; }

        public string PhysicalState { get; private set; }

        public string MeteringPointSubType { get; private set; }

        public string MeteringPointType { get; private set; }

        public string GridAreaId { get; private set; }

        public string? PowerPlantGsrnNumber { get; private set; }

        public string? LocationDescription { get; private set; }

        public string UnitType { get; private set; }

        public string MeterNumber { get; private set; }

        public string MeterReadingOccurrence { get; private set; }

        public int MaximumCurrent { get; private set; }

        public int MaximumPower { get; private set; }

        public bool ProductionObligation { get; private set; }

        public string NetSettlementGroup { get; private set; }

        public string DisconnectionType { get; private set; }

        public string ConnectionType { get; private set; }

        public string? AssetType { get; private set; }

        public string? ParentRelatedMeteringPoint { get; private set; }

        public string ProductType { get; private set; }
    }
}
