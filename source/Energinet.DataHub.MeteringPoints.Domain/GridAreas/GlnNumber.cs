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
using System.Collections.ObjectModel;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.GridAreas
{
    public class GlnNumber : ValueObject
    {
        public GlnNumber(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static GlnNumber Create(string glnNumber)
        {
            if (string.IsNullOrWhiteSpace(glnNumber)) throw new ArgumentException($"'{nameof(glnNumber)}' cannot be null or whitespace", nameof(glnNumber));

            var trimmedGlnNumber = glnNumber.Trim();

            var result = CheckRules(trimmedGlnNumber);
            if (!result.Success)
            {
                throw new InvalidOperationException("Invalid operator id.");
            }

            return new GlnNumber(trimmedGlnNumber);
        }

        public static BusinessRulesValidationResult CheckRules(string gsrnValue)
        {
            return new BusinessRulesValidationResult(new Collection<IBusinessRule>() { new GlnNumberFormatRule(gsrnValue), });
        }

        public override string ToString()
        {
            return Value;
        }

        private static void ThrowIfInvalid(string gsrnValue)
        {
            var result = CheckRules(gsrnValue);
            if (!result.Success)
            {
                throw new InvalidOperationException("Invalid supplier id.");
            }
        }
    }
}
