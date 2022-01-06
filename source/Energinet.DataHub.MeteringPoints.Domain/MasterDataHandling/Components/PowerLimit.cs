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
    public class PowerLimit : ValueObject
    {
        public PowerLimit(int? kwh, int? ampere)
        {
            Kwh = kwh;
            Ampere = ampere;
        }

        public int? Kwh { get; }

        public int? Ampere { get; }

        public static PowerLimit Create(int kwh, int ampere)
        {
            if (!CheckRules(kwh, ampere).Success)
            {
                throw new InvalidPowerLimitException();
            }

            return new PowerLimit(kwh, ampere);
        }

        public static BusinessRulesValidationResult CheckRules(int? kwh, int? ampere)
        {
           var rules = new Collection<IBusinessRule>()
           {
                new KwhPowerLimitRule(kwh),
                new AmperePowerLimitRule(ampere),
           };

           return new BusinessRulesValidationResult(rules);
        }
    }
}
