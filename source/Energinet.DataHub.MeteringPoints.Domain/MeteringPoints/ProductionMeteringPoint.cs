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

using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public class ProductionMeteringPoint : MeteringPoint
    {
        private int _productionObligation;
        private int _netSettlementGroup;
        private DisconnectionType _disconnectionType;
        private ConnectionType _connectionType;

        public ProductionMeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            string streetName,
            string postCode,
            string cityName,
            string countryCode,
            bool isAddressWashable,
            PhysicalState physicalState,
            MeteringPointSubType meteringPointSubType,
            MeteringPointType meteringPointType,
            GridAreaId gridAreaId,
            GsrnNumber powerPlantGsrnNumber,
            string locationDescription,
            string parentRelatedMeteringPoint,
            MeasurementUnitType unitType,
            string meterNumber,
            ReadingOccurrence meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            Instant? occurenceDate,
            int productionObligation,
            int netSettlementGroup,
            DisconnectionType disconnectionType,
            ConnectionType connectionType)
            : base(
                id,
                gsrnNumber,
                streetName,
                postCode,
                cityName,
                countryCode,
                isAddressWashable,
                physicalState,
                meteringPointSubType,
                meteringPointType,
                gridAreaId,
                powerPlantGsrnNumber,
                locationDescription,
                parentRelatedMeteringPoint,
                unitType,
                meterNumber,
                meterReadingOccurrence,
                maximumCurrent,
                maximumPower,
                occurenceDate)
        {
            _productionObligation = productionObligation;
            _netSettlementGroup = netSettlementGroup;
            _disconnectionType = disconnectionType;
            _connectionType = connectionType;
            _productType = ProductType.EnergyActive;
        }
    }
}
