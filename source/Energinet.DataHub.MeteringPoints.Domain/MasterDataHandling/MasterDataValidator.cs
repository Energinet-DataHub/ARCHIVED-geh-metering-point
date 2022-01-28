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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Exchange;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.ExchangeReactiveEnergy;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.NetConsumption;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.VEProduction;
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
                new SpecialMeteringPointValidator()
            },
            {
                MeteringPointType.InternalUse.Name,
                new InternalUseValidator()
            },
            {
                MeteringPointType.NetConsumption.Name,
                new NetConsumptionRules()
            },
            {
                MeteringPointType.NetProduction.Name,
                new SpecialMeteringPointValidator()
            },
            {
                MeteringPointType.OtherConsumption.Name,
                new OtherConsumptionMeteringPointValidator()
            },
            {
                MeteringPointType.OtherProduction.Name,
                new OtherProductionMeteringPointValidator()
            },
            {
                MeteringPointType.OwnProduction.Name,
                new SpecialMeteringPointValidator()
            },
            {
                MeteringPointType.TotalConsumption.Name,
                new TotalConsumption.Validator()
            },
            {
                MeteringPointType.WholesaleServices.Name,
                new SpecialMeteringPointValidator()
            },
            {
                MeteringPointType.ConsumptionFromGrid.Name,
                new SpecialMeteringPointValidator()
            },
            {
                MeteringPointType.ExchangeReactiveEnergy.Name,
                new ExchangeReactiveEnergyValidator()
            },
            {
                MeteringPointType.GridLossCorrection.Name,
                new SpecialMeteringPointValidator()
            },
            {
                MeteringPointType.NetFromGrid.Name,
                new SpecialMeteringPointValidator()
            },
            {
                MeteringPointType.NetToGrid.Name,
                new SpecialMeteringPointValidator()
            },
            {
                MeteringPointType.SupplyToGrid.Name,
                new SpecialMeteringPointValidator()
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
