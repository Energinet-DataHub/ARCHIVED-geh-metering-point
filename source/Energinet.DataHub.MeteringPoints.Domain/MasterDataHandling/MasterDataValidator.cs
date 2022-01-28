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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.ConsumptionFromGrid;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.ElectricalHeating;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Exchange;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.ExchangeReactiveEnergy;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.GridLossCorrection;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.NetConsumption;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.NetFromGrid;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.NetProduction;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.NetToGrid;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.OtherConsumption;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.OtherProduction;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.OwnProduction;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.SupplyToGrid;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.SurplusProduction;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.TotalConsumption;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.VEProduction;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.WholesaleServices;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling
{
    public class MasterDataValidator
    {
        private readonly Dictionary<string, IMasterDataValidatorStrategy> _validators = new Dictionary<string, IMasterDataValidatorStrategy>()
        {
            {
                MeteringPointType.Consumption.Name,
                new ConsumptionMeteringPointValidator()
            },
            {
                MeteringPointType.Production.Name,
                new ProductionMeteringPointValidator()
            },
            {
                MeteringPointType.Exchange.Name,
                new ExchangeMeteringPointValidator()
            },
            {
                MeteringPointType.Analysis.Name,
                new AnalysisValidator()
            },
            {
                MeteringPointType.ElectricalHeating.Name,
                new ElectricalHeatingValidator()
            },
            {
                MeteringPointType.InternalUse.Name,
                new InternalUseValidator()
            },
            {
                MeteringPointType.NetConsumption.Name,
                new NetConsumptionPointValidator()
            },
            {
                MeteringPointType.NetProduction.Name,
                new NetProductionValidator()
            },
            {
                MeteringPointType.OtherConsumption.Name,
                new OtherConsumptionValidator()
            },
            {
                MeteringPointType.OtherProduction.Name,
                new OtherProductionValidator()
            },
            {
                MeteringPointType.OwnProduction.Name,
                new OwnProductionValidator()
            },
            {
                MeteringPointType.TotalConsumption.Name,
                new TotalConsumptionValidator()
            },
            {
                MeteringPointType.WholesaleServices.Name,
                new WholesaleServicesValidator()
            },
            {
                MeteringPointType.ConsumptionFromGrid.Name,
                new ConsumptionFromGridValidator()
            },
            {
                MeteringPointType.ExchangeReactiveEnergy.Name,
                new ExchangeReactiveEnergyValidator()
            },
            {
                MeteringPointType.GridLossCorrection.Name,
                new GridLossCorrectionValidator()
            },
            {
                MeteringPointType.NetFromGrid.Name,
                new NetFromGridValidator()
            },
            {
                MeteringPointType.NetToGrid.Name,
                new NetToGridValidator()
            },
            {
                MeteringPointType.SupplyToGrid.Name,
                new SupplyToGridValidator()
            },
            {
                MeteringPointType.SurplusProductionGroup.Name,
                new SurplusProductionGroupValidator()
            },
            {
                MeteringPointType.VEProduction.Name,
                new VEProductionMeteringPointValidator()
            },
        };

        public BusinessRulesValidationResult CheckRulesFor(MeteringPointType type, MasterData masterData)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));
            return _validators[type.Name].CheckRules(masterData);
        }

        public BusinessRulesValidationResult CheckRulesFor(MeteringPoint meteringPoint, MasterData masterData)
        {
            if (meteringPoint is null) throw new ArgumentNullException(nameof(meteringPoint));
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));
            return _validators[meteringPoint.MeteringPointType.Name].CheckRules(meteringPoint, masterData);
        }
    }
}
