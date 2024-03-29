﻿// Copyright 2020 Energinet DataHub A/S
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
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.UnitType), Applicability.Required),
                    new(nameof(MasterData.AssetType), Applicability.Optional),
                    new(nameof(MasterData.SettlementMethod), Applicability.Required),
                    new(nameof(MasterData.ScheduledMeterReadingDate), Applicability.Optional),
                    new(nameof(MasterData.ReadingOccurrence), Applicability.Required),
                    new(nameof(MasterData.NetSettlementGroup), Applicability.Required),
                    new(nameof(MasterData.DisconnectionType), Applicability.Required),
                    new(nameof(MasterData.ConnectionType), Applicability.Optional),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.Production.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ScheduledMeterReadingDate), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.UnitType), Applicability.Required),
                    new(nameof(MasterData.NetSettlementGroup), Applicability.Required),
                    new(nameof(MasterData.ProductionObligation), Applicability.Required),
                }
            },
            {
                MeteringPointType.Exchange.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.UnitType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.VEProduction.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.UnitType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.Analysis.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.ElectricalHeating.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.InternalUse.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.NetConsumption.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.NetProduction.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.OtherConsumption.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.OtherProduction.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.OwnProduction.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.TotalConsumption.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.WholesaleServices.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.ConsumptionFromGrid.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.ExchangeReactiveEnergy.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.GridLossCorrection.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.NetFromGrid.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.NetToGrid.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.SupplyToGrid.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
                }
            },
            {
                MeteringPointType.SurplusProductionGroup.Name, new List<MasterDataField>()
                {
                    new(nameof(MasterData.ProductType), Applicability.Required),
                    new(nameof(MasterData.SettlementMethod), Applicability.NotAllowed),
                    new(nameof(MasterData.ProductionObligation), Applicability.NotAllowed),
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
