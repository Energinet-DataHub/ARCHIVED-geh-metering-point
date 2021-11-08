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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public class MeterId : ValueObject
    {
        private MeterId(string meterId)
        {
            Value = meterId;
        }

        public string Value { get; }

        public static BusinessRulesValidationResult CheckRules(string meterId)
        {
            var rules = new List<IBusinessRule>()
            {
                new MeterIdLengthRule(meterId),
            };

            return new BusinessRulesValidationResult(rules);
        }

        public static MeterId Create(string meterId)
        {
            if (CheckRules(meterId).Success == false)
            {
                throw new InvalidMeterIdException();
            }

            return new MeterId(meterId);
        }

        public static MeterId NotSet()
        {
            return new MeterId(string.Empty);
        }
    }
}
