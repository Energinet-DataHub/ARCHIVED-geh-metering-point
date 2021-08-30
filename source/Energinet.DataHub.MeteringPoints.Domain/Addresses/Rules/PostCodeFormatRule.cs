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
using System.Text.RegularExpressions;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.Addresses.Rules
{
    public class PostCodeFormatRule : IBusinessRule
    {
        private const int MaxLength = 10;
        private const string DanishCountryCode = "DK";
        private const string DanishRegExRule = @"^([0-9]{4})$";

        public PostCodeFormatRule(string? postCode, string? countryCode)
        {
            Validate(postCode, countryCode);
        }

        public bool IsBroken { get; private set; }

        public ValidationError ValidationError { get; private set; } = new PostCodeFormatRuleError();

        private static bool IsDanish(string? countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                return false;
            }

            return countryCode.Equals(DanishCountryCode, StringComparison.OrdinalIgnoreCase);
        }

        private void Validate(string? postCode, string? countryCode)
        {
            if (string.IsNullOrWhiteSpace(postCode))
            {
                return;
            }

            if (IsDanish(countryCode))
            {
                ValidateDanishPostCode(postCode);
            }
            else
            {
                ValidatePostCode(postCode);
            }
        }

        private void ValidateDanishPostCode(string postCode)
        {
            if (Regex.IsMatch(postCode!, DanishRegExRule) == false)
            {
                IsBroken = true;
                ValidationError = new PostCodeFormatRuleError(postCode, MaxLength);
            }
        }

        private void ValidatePostCode(string postCode)
        {
            if (postCode.Length > MaxLength)
            {
                IsBroken = true;
                ValidationError = new PostCodeFormatRuleError(postCode, MaxLength);
            }
        }
    }
}
