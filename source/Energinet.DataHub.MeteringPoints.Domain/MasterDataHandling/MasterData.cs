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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling
{
    public class MasterData : ValueObject
    {
        public MasterData(ProductType productType, MeasurementUnitType unitType, AssetType? assetType, ReadingOccurrence readingOccurrence, PowerLimit powerLimit, GsrnNumber? powerPlantGsrnNumber, EffectiveDate effectiveDate, Capacity? capacity, Address address, MeteringConfiguration meteringConfiguration, SettlementMethod settlementMethod, ScheduledMeterReadingDate? scheduledMeterReadingDate, ConnectionType? connectionType, DisconnectionType disconnectionType, NetSettlementGroup netSettlementGroup, bool? productionObligation)
        {
            ProductType = productType;
            UnitType = unitType;
            AssetType = assetType;
            ReadingOccurrence = readingOccurrence;
            PowerLimit = powerLimit;
            PowerPlantGsrnNumber = powerPlantGsrnNumber;
            EffectiveDate = effectiveDate;
            Capacity = capacity;
            Address = address;
            MeteringConfiguration = meteringConfiguration;
            SettlementMethod = settlementMethod;
            ScheduledMeterReadingDate = scheduledMeterReadingDate;
            ConnectionType = connectionType;
            DisconnectionType = disconnectionType;
            NetSettlementGroup = netSettlementGroup;
            ProductionObligation = productionObligation;
        }

        private MasterData()
        {
            ProductType = ProductType.EnergyActive;
            UnitType = MeasurementUnitType.KWh;
            ReadingOccurrence = ReadingOccurrence.Quarterly;
            PowerLimit = PowerLimit.Create(0, 0);
            Address = Address.Create();
            MeteringConfiguration = MeteringConfiguration.Create(MeteringMethod.Virtual, MeterId.Empty());
            DisconnectionType = DisconnectionType.Manual;
        }

        public ProductType ProductType { get; }

        public MeasurementUnitType UnitType { get; }

        public AssetType? AssetType { get; }

        public ReadingOccurrence ReadingOccurrence { get; }

        public PowerLimit PowerLimit { get; }

        public GsrnNumber? PowerPlantGsrnNumber { get; }

        public EffectiveDate? EffectiveDate { get; }

        public Capacity? Capacity { get; }

        public Address Address { get; }

        public MeteringConfiguration MeteringConfiguration { get; }

        public SettlementMethod? SettlementMethod { get; }

        public ScheduledMeterReadingDate? ScheduledMeterReadingDate { get; }

        public ConnectionType? ConnectionType { get; }

        public DisconnectionType? DisconnectionType { get; }

        public NetSettlementGroup? NetSettlementGroup { get; }

        public bool? ProductionObligation { get; }
    }
}
