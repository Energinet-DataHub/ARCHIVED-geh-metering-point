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

using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling
{
    /// <summary>
    /// Validates a master data for specific type of metering point
    /// </summary>
    internal interface IMasterDataValidatorStrategy
    {
        /// <summary>
        /// The type of metering point this validator targets
        /// </summary>
        MeteringPointType Target { get; }

        /// <summary>
        /// Checks business rules and returns the result
        /// </summary>
        /// <param name="masterData"></param>
        /// <returns><see cref="BusinessRulesValidationResult"/></returns>
        BusinessRulesValidationResult CheckRules(MasterData masterData);

        /// <summary>
        /// Verifies if master data is valid for a particular metering point
        /// </summary>
        /// <param name="meteringPoint"></param>
        /// <param name="updatedMasterData"></param>
        /// <returns><see cref="BusinessRulesValidationResult"/></returns>
        BusinessRulesValidationResult CheckRules(MeteringPoint meteringPoint, MasterData updatedMasterData);
    }
}
