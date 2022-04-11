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
using System.Globalization;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components
{
    public class PowerLimit : ValueObject
    {
        private PowerLimit(int? kwh, int? ampere)
        {
            Kwh = kwh;
            Ampere = ampere;
        }

        public int? Kwh { get; }

        public int? Ampere { get; }

        public static PowerLimit Create(int? kwh, int? ampere)
        {
            var kwhAsString = ConvertNullableInt(kwh);
            var ampereAsString = ConvertNullableInt(ampere);
            return Create(kwhAsString, ampereAsString);
        }

        public static PowerLimit Create(string? kwh, string? ampere)
        {
            var kwhAsInt = ConvertNullableString(kwh);
            var ampereAsInt = ConvertNullableString(ampere);
            if (!CheckRules(kwhAsInt, ampereAsInt).Success)
            {
                throw new InvalidPowerLimitException();
            }

            return new PowerLimit(kwhAsInt, ampereAsInt);
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

        public static BusinessRulesValidationResult CheckRules(string? kwh, string? ampere)
        {
            var kwhAsInt = ConvertNullableString(kwh);
            var ampereAsInt = ConvertNullableString(ampere);
            return CheckRules(kwhAsInt, ampereAsInt);
        }

        private static string? ConvertNullableInt(int? input)
        {
            if (input is null) return null;
            return Convert.ToString(input, CultureInfo.InvariantCulture);
        }

        private static int? ConvertNullableString(string? input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            return Convert.ToInt32(input, CultureInfo.InvariantCulture);
        }
    }
}
