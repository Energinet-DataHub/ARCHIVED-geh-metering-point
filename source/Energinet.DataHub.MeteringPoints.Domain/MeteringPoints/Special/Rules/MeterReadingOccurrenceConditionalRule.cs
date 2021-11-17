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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Special.Rules
{
    public class MeterReadingOccurrenceConditionalRule : IBusinessRule
    {
        public MeterReadingOccurrenceConditionalRule(ReadingOccurrence readingOccurrence, MeteringPointType meteringPointType)
        {
            if (meteringPointType == null!) throw new ArgumentNullException(nameof(meteringPointType));
            if (readingOccurrence == null!) throw new ArgumentNullException(nameof(readingOccurrence));
            IsBroken = !ValidateRule(readingOccurrence, meteringPointType);
            ValidationError = new InvalidMeterReadingOccurrenceRuleError(readingOccurrence.Name);
        }

        public bool IsBroken { get; }

        public ValidationError ValidationError { get; }

        private static bool ValidateRule(ReadingOccurrence readingOccurrence, MeteringPointType meteringPointType)
        {
            var t = meteringPointType.Name;

            return meteringPointType.Name switch
            {
                nameof(MeteringPointType.VEProduction) => readingOccurrence == ReadingOccurrence.Hourly || readingOccurrence == ReadingOccurrence.Quarterly || readingOccurrence == ReadingOccurrence.Monthly,
                _ => readingOccurrence == ReadingOccurrence.Hourly || readingOccurrence == ReadingOccurrence.Quarterly,
            };
        }
    }
}
