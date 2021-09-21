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

using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public class ProductionMeteringPoint : MeteringPoint
    {
        private NetSettlementGroup _netSettlementGroup;
        private DisconnectionType _disconnectionType;
        private ConnectionType _connectionType;
        private bool _isAddressWashable;
#pragma warning disable 414 // TODO: Temporarily disabled since variable is not yet in use
        private bool _productionObligation;
#pragma warning restore 414

        public ProductionMeteringPoint(
            MeteringPointId id,
            GsrnNumber gsrnNumber,
            Address address,
            bool isAddressWashable,
            MeteringPointSubType meteringPointSubType,
            MeteringPointType meteringPointType,
            GridAreaId gridAreaId,
            GsrnNumber? powerPlantGsrnNumber,
            string? locationDescription,
            MeasurementUnitType unitType,
            string? meterNumber,
            ReadingOccurrence meterReadingOccurrence,
            int maximumCurrent,
            int maximumPower,
            EffectiveDate effectiveDate,
            NetSettlementGroup netSettlementGroup,
            DisconnectionType disconnectionType,
            ConnectionType connectionType,
            string? parentRelatedMeteringPoint,
            ProductType productType)
            : base(
                id,
                gsrnNumber,
                address,
                meteringPointSubType,
                meteringPointType,
                gridAreaId,
                powerPlantGsrnNumber,
                locationDescription,
                unitType,
                meterNumber,
                meterReadingOccurrence,
                maximumCurrent,
                maximumPower,
                effectiveDate,
                parentRelatedMeteringPoint)
        {
            _netSettlementGroup = netSettlementGroup;
            _disconnectionType = disconnectionType;
            _connectionType = connectionType;
            _productionObligation = false;
            _productType = productType;
            _isAddressWashable = isAddressWashable;

            AddDomainEvent(new MeteringPointCreated(id, GsrnNumber, meteringPointType, gridAreaId, meteringPointSubType, ConnectionState.PhysicalState, meterReadingOccurrence, ProductType.Tariff, unitType, netSettlementGroup));
        }

#pragma warning disable 8618 // Must have an empty constructor, since EF cannot bind Address in main constructor
        private ProductionMeteringPoint() { }
#pragma warning restore 8618

        public override BusinessRulesValidationResult ConnectAcceptable(ConnectionDetails connectionDetails)
        {
            throw new System.NotImplementedException();
        }

        public override void Connect(ConnectionDetails connectionDetails)
        {
            throw new System.NotImplementedException();
        }
    }
}
