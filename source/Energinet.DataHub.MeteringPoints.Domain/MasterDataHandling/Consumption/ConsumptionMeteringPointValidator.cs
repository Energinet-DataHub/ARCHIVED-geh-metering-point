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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using PowerPlantIsRequiredForNetSettlementGroupRule = Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Consumption.Rules.PowerPlantIsRequiredForNetSettlementGroupRule;
using StreetNameIsRequiredRule = Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules.StreetNameIsRequiredRule;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Consumption
{
    internal class ConsumptionMeteringPointValidator : IMasterDataValidatorStrategy
    {
        public BusinessRulesValidationResult CheckRules(MasterData masterData)
        {
            return new BusinessRulesValidationResult(new List<IBusinessRule>()
            {
                new MeterReadingOccurrenceRule(masterData.ReadingOccurrence),
                new CityIsRequiredRule(masterData.Address),
                new StreetNameIsRequiredRule(masterData.Address),
                new PostCodeIsRequiredRule(masterData.Address),
                new GeoInfoReferenceRequirementRule(masterData.Address),
                new MeteringMethodRule(masterData.NetSettlementGroup!, masterData.MeteringConfiguration.Method),
                new ScheduledMeterReadingDateRule(masterData.ScheduledMeterReadingDate, masterData.NetSettlementGroup!),
                new PowerPlantIsRequiredForNetSettlementGroupRule(masterData.NetSettlementGroup!, masterData.PowerPlantGsrnNumber),
                new CapacityRequirementRule(masterData.Capacity, masterData.NetSettlementGroup!),
                new AssetTypeRequirementRule(masterData.AssetType, masterData.NetSettlementGroup!),
                new ConnectionTypeRequirementRule(masterData.NetSettlementGroup!, masterData.ConnectionType),
                new SettlementMethodMustBeFlexOrNonProfiledRule(masterData.SettlementMethod!),
            });
        }
    }
}
