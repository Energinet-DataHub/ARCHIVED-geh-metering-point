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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling
{
    public class MasterDataValidator
    {
        private readonly Dictionary<string, IMasterDataValidatorStrategy> _validators = new();

        public MasterDataValidator(params IMasterDataValidatorStrategy[] validators)
        {
            if (validators == null) throw new ArgumentNullException(nameof(validators));
            foreach (var masterDataValidatorStrategy in validators)
            {
                _validators.Add(masterDataValidatorStrategy.Target.Name, masterDataValidatorStrategy);
            }
        }

        public BusinessRulesValidationResult CheckRulesFor(MeteringPointType type, MasterData masterData)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));
            return _validators[type.Name].CheckRules(masterData);
        }

        internal BusinessRulesValidationResult CheckRulesFor(MeteringPoint meteringPoint, MasterData masterData)
        {
            if (meteringPoint is null) throw new ArgumentNullException(nameof(meteringPoint));
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));
            return _validators[meteringPoint.MeteringPointType.Name].CheckRules(meteringPoint, masterData);
        }
    }
}
