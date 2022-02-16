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

namespace Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Fixtures
{
    public static class TestDataCreator
    {
        private static readonly Random _random = new();

        public static string CreateGsrn()
        {
            var prefix = "5713131";
            var withBody = AddBody(prefix);
            var gsrn = AddChecksum(withBody);
            return gsrn;
        }

        public static string Today()
        {
            var date = DateTime.Today.AddDays(-1);
            if (TimeZoneInfo.Utc.IsDaylightSavingTime(date))
            {
                return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "T23:00:00Z";
            }
            else
            {
                return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "T22:00:00Z";
            }
        }

        private static int Parse(string input)
        {
            return int.Parse(input, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
        }

        private static string AddBody(string prefix)
        {
#pragma warning disable CA5394 // No need to use cryptographically secure numbers here.
            return prefix.Length < 17
                ? AddBody(prefix + _random.Next(0, 10).ToString(CultureInfo.InvariantCulture))
                : prefix;
#pragma warning restore CA5394
        }

        private static string AddChecksum(string body)
        {
            var sum = 0;
            var positionIsOdd = true;
            for (var currentPosition = 1; currentPosition < 18; currentPosition++)
            {
                var currentValueAtPosition = Parse(body.Substring(currentPosition - 1, 1));
                if (positionIsOdd)
                {
                    sum += currentValueAtPosition * 3;
                }
                else
                {
                    sum += currentValueAtPosition * 1;
                }

                positionIsOdd = !positionIsOdd;
            }

            var equalOrHigherMultipleOf = (int)(Math.Ceiling(sum / 10.0) * 10);

            return body + (equalOrHigherMultipleOf - sum);
        }
    }
}
