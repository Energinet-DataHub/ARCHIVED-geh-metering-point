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

using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using StreetNameIsRequiredRule = Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules.StreetNameIsRequiredRule;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.NetProduction
{
    internal class NetProductionValidator : IMasterDataValidatorStrategy
    {
        public BusinessRulesValidationResult CheckRules(MasterData masterData)
        {
            return new BusinessRulesValidationResult(new List<IBusinessRule>()
            {
                new StreetNameIsRequiredRule(masterData.Address),
                new MeterReadingOccurrenceRule(masterData.ReadingOccurrence),
                new ProductTypeMustBeEnergyActiveRule(masterData.ProductType),
                new UnitTypeMustBeKwh(masterData.UnitType),
            });
        }

        public BusinessRulesValidationResult CheckRules(MeteringPoint meteringPoint, MasterData updatedMasterData)
        {
            return CheckRules(updatedMasterData);
        }
    }
}
