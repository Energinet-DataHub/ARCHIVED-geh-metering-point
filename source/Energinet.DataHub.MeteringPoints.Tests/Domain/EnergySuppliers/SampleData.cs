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
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Tests.Domain.EnergySuppliers
{
    public static class SampleData
    {
        public static Instant Today => Instant.FromUtc(2021, 1, 1, 0, 0);

        public static Instant TodayPlusOne => Instant.FromUtc(2021, 1, 2, 0, 0);

        public static Instant TodayPlusTwo => Instant.FromUtc(2021, 1, 3, 0, 0);

        public static Instant TodayPlusThree => Instant.FromUtc(2021, 1, 4, 0, 0);

        public static Instant TodayPlusFour => Instant.FromUtc(2021, 1, 5, 0, 0);

        public static Instant TodayPlusFive => Instant.FromUtc(2021, 1, 6, 0, 0);

        public static string Transaction => Guid.NewGuid().ToString();
    }
}
