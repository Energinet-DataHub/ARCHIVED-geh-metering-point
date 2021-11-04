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
using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.Policies
{
    public static class TimePeriodPolicy
    {
        public static BusinessRulesValidationResult Check(Instant today, EffectiveDate effectiveDate)
        {
            if (effectiveDate == null) throw new ArgumentNullException(nameof(effectiveDate));

            if (!EffectiveDateIsWithinAllowedTimePeriod(DifferenceInDays(today, effectiveDate)))
            {
                return new BusinessRulesValidationResult(new List<ValidationError>()
                {
                    new EffectiveDateIsNotWithinAllowedTimePeriod(),
                });
            }
            else
            {
                return new BusinessRulesValidationResult(new List<ValidationError>());
            }
        }

        private static bool EffectiveDateIsWithinAllowedTimePeriod(TimeSpan diff)
        {
            return diff.Days is 0 or 1;
        }

        private static TimeSpan DifferenceInDays(Instant today, EffectiveDate effectiveDate)
        {
            return ToDate(today) - ToDate(effectiveDate.DateInUtc);
        }

        private static DateTime ToDate(Instant instant)
        {
            return instant.ToDateTimeUtc().Date;
        }
    }
}
