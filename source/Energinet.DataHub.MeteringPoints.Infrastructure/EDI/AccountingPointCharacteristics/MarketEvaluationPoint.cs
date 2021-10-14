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
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common.Address;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics
{
    public record MarketEvaluationPoint(
        Mrid Id,
        MarketParticipant MeteringPointResponsibleMarketRoleParticipant,
        string Type,
        string SettlementMethod,
        string MeteringMethod,
        string ConnectionState,
        string ReadCycle,
        string NetSettlementGroup,
        string NextReadingDate,
        Mrid MeteringGridAreaDomainId,
        Mrid InMeteringGridAreaDomainId,
        Mrid OutMeteringGridAreaDomainId,
        Mrid LinkedMarketEvaluationPoint,
        UnitValue PhysicalConnectionCapacity,
        string ConnectionType,
        string DisconnectionMethod,
        string AssetMarketPSRTypePsrType,
        bool ProductionObligation,
        Series Series,
        UnitValue ContractedConnectionCapacity,
        UnitValue RatedCurrent,
        string MeterId,
        MarketParticipant EnergySupplierMarketParticipantId,
        DateTime SupplyStartDateAndOrTimeDateTime,
        string Description,
        MainAddress UsagePointLocationMainAddress,
        bool UsagePointLocationActualAddressIndicator,
        string UsagePointLocationGeoInfoReference,
        ParentMarketEvaluationPoint ParentMarketEvaluationPointId,
        ChildMarketEvaluationPoint ChildMarketEvaluationPoint);
}
