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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling
{
    public abstract class MasterDataBuilderBase
    {
        private readonly List<MasterDataValue> _values = new()
        {
            new MasterDataValue(nameof(MasterData.ProductType), typeof(ProductType), Applicability.Required),
            new MasterDataValue(nameof(MasterData.UnitType), typeof(MeasurementUnitType), Applicability.Required),
            new MasterDataValue(nameof(MasterData.AssetType), typeof(AssetType), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.SettlementMethod), typeof(SettlementMethod), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.ScheduledMeterReadingDate), typeof(ScheduledMeterReadingDate), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.MeteringConfiguration), typeof(MeteringConfiguration), Applicability.Required),
            new MasterDataValue(nameof(MasterData.ReadingOccurrence), typeof(ReadingOccurrence), Applicability.Required),
            new MasterDataValue(nameof(MasterData.NetSettlementGroup), typeof(NetSettlementGroup), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.PowerLimit), typeof(PowerLimit), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.PowerPlantGsrnNumber), typeof(GsrnNumber), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.DisconnectionType), typeof(DisconnectionType), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.ConnectionType), typeof(ConnectionType), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.EffectiveDate), typeof(EffectiveDate), Applicability.Required),
            new MasterDataValue(nameof(MasterData.Capacity), typeof(Capacity), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.Address), typeof(Address), Applicability.Optional),
            new MasterDataValue(nameof(MasterData.ProductionObligation), typeof(bool), Applicability.Optional),
        };

        protected MasterDataBuilderBase(IEnumerable<MasterDataField> masterDataFields)
        {
            masterDataFields.ToList().ForEach(field => ConfigureApplicability(field.Name, field.Applicability));
        }

        public MasterData Build()
        {
            return new MasterData(
                productType: GetValue<ProductType>(nameof(MasterData.ProductType)),
                unitType: GetValue<MeasurementUnitType>(nameof(MasterData.UnitType)),
                assetType: GetValue<AssetType>(nameof(MasterData.AssetType)),
                readingOccurrence: GetValue<ReadingOccurrence>(nameof(MasterData.ReadingOccurrence)),
                powerLimit: GetValue<PowerLimit>(nameof(MasterData.PowerLimit)),
                powerPlantGsrnNumber: GetValue<GsrnNumber>(nameof(MasterData.PowerPlantGsrnNumber)),
                effectiveDate: GetValue<EffectiveDate>(nameof(MasterData.EffectiveDate)),
                capacity: GetValue<Capacity>(nameof(MasterData.Capacity)),
                address: GetValue<Address>(nameof(MasterData.Address)),
                meteringConfiguration: GetValue<MeteringConfiguration>(nameof(MasterData.MeteringConfiguration)),
                settlementMethod: GetValue<SettlementMethod>(nameof(MasterData.SettlementMethod)),
                scheduledMeterReadingDate: GetValue<ScheduledMeterReadingDate>(nameof(MasterData.ScheduledMeterReadingDate)),
                connectionType: GetValue<ConnectionType>(nameof(MasterData.ConnectionType)),
                disconnectionType: GetValue<DisconnectionType>(nameof(MasterData.DisconnectionType)),
                netSettlementGroup: GetValue<NetSettlementGroup>(nameof(MasterData.NetSettlementGroup)),
                productionObligation: GetValue<bool?>(nameof(MasterData.ProductionObligation)));
        }

        protected void SetValue<T>(string name, T? value)
        {
            var valueItem = GetMasterValueItem<T>(name);
            if (valueItem.Applicability != Applicability.NotAllowed)
            {
                valueItem.SetValue(value);
            }
        }

        protected T GetValue<T>(string name)
        {
            return (T)GetMasterValueItem<T>(name).Value!;
        }

        protected MasterDataValue GetMasterValueItem<T>(string name)
        {
            return _values.First(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        protected IEnumerable<ValidationError> AllValueValidationErrors()
        {
            return _values.SelectMany(value => value.ValidationErrors).AsEnumerable();
        }

        private void ConfigureApplicability(string name, Applicability applicability)
        {
            var value = _values.First(value => value.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            value.SetApplicability(applicability);
        }
    }
}
