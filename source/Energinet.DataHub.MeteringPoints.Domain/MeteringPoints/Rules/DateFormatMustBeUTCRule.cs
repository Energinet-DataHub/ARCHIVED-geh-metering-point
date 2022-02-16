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
using System.Text;
using System.Text.RegularExpressions;
using Energinet.DataHub.MeteringPoints.Domain.Extensions;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;
using NodaTime.Text;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules
{
    public class DateFormatMustBeUTCRule : IBusinessRule
    {
        private const string FormatRegExSummer = @"\d{4}-(?:0[1-9]|1[0-2])-(?:0[1-9]|[1-2]\d|3[0-1])T23:00:00(.000)?Z$";
        private const string FormatRegExWinter = @"\d{4}-(?:0[1-9]|1[0-2])-(?:0[1-9]|[1-2]\d|3[0-1])T22:00:00(.000)?Z$";
        private readonly string _date;

        public DateFormatMustBeUTCRule(string date)
        {
            _date = date;
            var canParse = DateTime.TryParse(
                date,
                out var parseSuccess);

            if (!canParse)
            {
                IsBroken = true;
                return;
            }

            var info = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var tzi = info.IsDaylightSavingTime(parseSuccess);

            if (tzi)
            {
                IsBroken = !Regex.IsMatch(date, FormatRegExSummer);
            }
            else
            {
                IsBroken = !Regex.IsMatch(date, FormatRegExWinter);
            }
        }

        public bool IsBroken { get; }

        public ValidationError ValidationError => new DateFormatMustBeUTCRuleError(_date);
    }
}
