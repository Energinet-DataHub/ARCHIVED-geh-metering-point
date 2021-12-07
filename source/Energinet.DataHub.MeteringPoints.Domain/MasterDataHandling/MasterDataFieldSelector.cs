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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling
{
    public class MasterDataFieldSelector
    {
        private readonly Dictionary<string, List<MasterDataField>> _fields = new()
        {
            {
                MeteringPointType.Consumption.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required, false, ProductType.EnergyActive),
                    new(nameof(MasterData.UnitType), Applicability.Required, false, MeasurementUnitType.KWh),
                    new(nameof(MasterData.AssetType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.Optional),
                    new(nameof(MasterData.ScheduledMeterReadingDate), Applicability.Optional),
                    new(nameof(MasterData.ReadingOccurrence), Applicability.Required),
                    new(nameof(MasterData.NetSettlementGroup), Applicability.Required),
                    new(nameof(MasterData.DisconnectionType), Applicability.Required),
                    new(nameof(MasterData.ConnectionType), Applicability.Optional),
                }
            },
            {
                MeteringPointType.Production.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ScheduledMeterReadingDate), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductType), Applicability.Required, false, ProductType.EnergyActive),
                    new(nameof(MasterData.UnitType), Applicability.Required, true, MeasurementUnitType.KWh),
                }
            },
            {
                MeteringPointType.Exchange.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required, false, ProductType.EnergyActive),
                    new(nameof(MasterData.UnitType), Applicability.Required, true, MeasurementUnitType.KWh),
                }
            },
            {
                MeteringPointType.VEProduction.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required, false, ProductType.EnergyActive),
                }
            },
        };

        public IEnumerable<MasterDataField> GetMasterDataFieldsFor(MeteringPointType meteringPointType)
        {
            if (meteringPointType is null) throw new ArgumentNullException(nameof(meteringPointType));
            if (_fields.ContainsKey(meteringPointType.Name))
            {
                return _fields[meteringPointType.Name];
            }

            return new List<MasterDataField>().AsReadOnly();
        }
    }
}
