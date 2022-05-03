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

using System.Collections.ObjectModel;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components
{
    public class DisconnectionType : EnumerationType
    {
        public static readonly DisconnectionType Remote = new DisconnectionType(0, nameof(Remote));
        public static readonly DisconnectionType Manual = new DisconnectionType(1, nameof(Manual));

        private DisconnectionType(int id, string name)
            : base(id, name)
        {
        }

        public static BusinessRulesValidationResult CheckRules(string meteringPointType, string disconnectionType, string gsrnNumber)
        {
            var rules = new Collection<IBusinessRule>()
            {
                new DisconnectionTypeMandatory(meteringPointType, disconnectionType, gsrnNumber),
                new DisconnectionTypeIsValidRule(meteringPointType, disconnectionType),
            };

            return new BusinessRulesValidationResult(rules);
        }
    }
}
