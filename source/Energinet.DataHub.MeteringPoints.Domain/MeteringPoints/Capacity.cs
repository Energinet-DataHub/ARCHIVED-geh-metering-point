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
using System.Globalization;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public class Capacity : ValueObject
    {
        private Capacity(float kwh)
        {
            Kwh = kwh;
        }

        public float Kwh { get; }

        public static Capacity Create(string capacityInKwh)
        {
            if (CheckRules(capacityInKwh).Success == false)
            {
                throw new InvalidCapacityExeception(capacityInKwh);
            }

            var convertedValue = float.Parse(capacityInKwh, CultureInfo.InvariantCulture);
            return new Capacity(convertedValue);
        }

        public static BusinessRulesValidationResult CheckRules(string capacityInKwh)
        {
            var rules = new List<IBusinessRule>()
            {
                new CapcityFormatRule(capacityInKwh),
            };

            return new BusinessRulesValidationResult(rules);
        }
    }
}
