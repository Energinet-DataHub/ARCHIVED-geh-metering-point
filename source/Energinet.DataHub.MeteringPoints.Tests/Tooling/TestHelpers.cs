﻿// Copyright 2020 Energinet DataHub A/S
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
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Tests.Tooling
{
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

            var info = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var isDaylightSavingTime = info.IsDaylightSavingTime(dateForString);

            var retVal = dateForString.ToString(
                isDaylightSavingTime
                ? $"yyyy'-'MM'-'dd'T'22':'mm':'ss'Z'"
                : "yyyy'-'MM'-'dd'T'23':'mm':'ss'Z'",
                CultureInfo.InvariantCulture);

            return retVal;
        }

        public static Instant DaylightSavingsInstant(DateTime date)
        {
            var info = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var isDaylightSavingTime = info.IsDaylightSavingTime(date);

            return Instant.FromUtc(
                date.Year,
                date.Month,
                date.Day,
                isDaylightSavingTime ? 22 : 23,
                0);
        }

        public static DateTime DaylightSavingsAdjusted(DateTime date)
        {
            var info = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var isDaylightSavingTime = info.IsDaylightSavingTime(date);

            return new DateTime(
                date.Year,
                date.Month,
                date.Day,
                isDaylightSavingTime ? 22 : 23,
                0,
                0);
        }
    }
}
