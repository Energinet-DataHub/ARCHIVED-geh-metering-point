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

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules
{
    public class StreetCodeRule : IBusinessRule
    {
        private readonly string _streetCode;

        public StreetCodeRule(string streetCode)
        {
            _streetCode = streetCode;
        }

        public bool IsBroken => !IsDarCompliant(_streetCode);

        public ValidationError ValidationError => new InvalidStreetCodeError(_streetCode);

        private static bool IsDarCompliant(string streetCode)
        {
            if (streetCode.Length != 4)
            {
                return false;
            }

            if (int.TryParse(streetCode, out var numericStreetCode) == false)
            {
                return false;
            }

            return numericStreetCode is > 0 and <= 9999;
        }
    }
}
