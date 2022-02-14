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
using FluentAssertions.Extensions;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling
{
#pragma warning disable CA1305
    public static class TestHelpers
    {
        public static string DaylightSavingsString(int minute = 0, int second = 0, int millisecond = 0)
        {
            var now = DateTime.Now;
            return DaylightSavingsString(new DateTime(
                now.Year,
                now.Month,
                now.Day,
                now.Hour,
                minute,
                second,
                millisecond));
        }

        public static string DaylightSavingsString(DateTime date)
        {
            // setting the hour to 20 so that dates will never change
            var dateForString = new DateTime(
                date.Year,
                date.Month,
                date.Day,
                20,
                date.Minute,
                date.Second,
                date.Millisecond);

            var retVal = dateForString.ToString(TimeZoneInfo.Local.IsDaylightSavingTime(date)
                ? $"yyyy'-'MM'-'dd'T'23':'mm':'ss'Z'"
                : "yyyy'-'MM'-'dd'T'22':'mm':'ss'Z'");

            return retVal;
        }

        public static Instant DaylightSavingsInstant(DateTime date)
        {
            return Instant.FromUtc(
                date.Year,
                date.Month,
                date.Day,
                TimeZoneInfo.Local.IsDaylightSavingTime(date) ? 23 : 22,
                0);
        }
    }
}
