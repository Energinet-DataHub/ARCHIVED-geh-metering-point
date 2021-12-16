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
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;

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
            masterDataFields.ToList().ForEach(field => ConfigureValue(field.Name, field.Applicability, field.DefaultValue, field.CanBeChanged));
        }

        protected void SetValue<T>(string name, T value)
        {
            var valueItem = GetMasterValueItem<T>(name);
            if (valueItem.CanBeChanged && valueItem.Applicability != Applicability.NotAllowed)
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

        private void ConfigureValue<T>(string name, Applicability applicability, T? defaultValue = default(T), bool canBeChanged = true)
        {
            var value = GetMasterValueItem<T>(name);

            if (canBeChanged == false)
            {
                value.CanNotBeChanged();
            }

            value.SetValue(defaultValue);
            value.SetApplicability(applicability);
        }
    }
}
