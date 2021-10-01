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
using System.Globalization;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules
{
    public class CapacityFormatRule : IBusinessRule
    {
        private const int MaxLength = 9;

        public CapacityFormatRule(string capacityInKw)
        {
            if (capacityInKw == null) throw new ArgumentNullException(nameof(capacityInKw));
            ValidationError = new InvalidCapacityFormatRuleError(capacityInKw);

            if (CanParse(capacityInKw) == false)
            {
                IsBroken = true;
                return;
            }

            var length = GetLength(capacityInKw);
            if (length > MaxLength)
            {
                IsBroken = true;
            }
        }

        public bool IsBroken { get; }

        public ValidationError ValidationError { get; }

        private static int GetLength(string capacityInKw)
        {
            return !capacityInKw.Contains('.', StringComparison.OrdinalIgnoreCase) ? capacityInKw.Length : capacityInKw.Remove(capacityInKw.IndexOf('.', StringComparison.OrdinalIgnoreCase), 1).Length;
        }

        private static bool CanParse(string value)
        {
            return float.TryParse(value, out _);
        }
    }
}
