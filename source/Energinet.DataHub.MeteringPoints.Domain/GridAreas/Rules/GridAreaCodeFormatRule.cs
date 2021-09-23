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

using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.GridAreas.Rules
{
    public class GridAreaCodeFormatRule : IBusinessRule
    {
        private readonly string? _gridAreaCodeValue;

        public GridAreaCodeFormatRule(string? gridAreaCodeValue)
        {
            _gridAreaCodeValue = gridAreaCodeValue;
        }

        public bool IsBroken => !IsValid;

        public ValidationError ValidationError => new GridAreaCodeFormatRuleError();

        private bool IsValid => HasCorrectLength() && IsDigits();

        private bool HasCorrectLength()
        {
            return _gridAreaCodeValue?.Length == 3;
        }

        private bool IsDigits()
        {
            return short.TryParse(
                _gridAreaCodeValue,
                System.Globalization.NumberStyles.Integer,
                System.Globalization.NumberFormatInfo.InvariantInfo,
                out _);
        }
    }
}
