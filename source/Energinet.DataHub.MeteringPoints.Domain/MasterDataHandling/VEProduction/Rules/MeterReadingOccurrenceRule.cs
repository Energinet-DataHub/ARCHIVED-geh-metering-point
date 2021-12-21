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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.VEProduction.Rules
{
    public class MeterReadingOccurrenceRule : IBusinessRule
    {
        public MeterReadingOccurrenceRule(ReadingOccurrence readingOccurrence)
        {
            if (readingOccurrence == null!) throw new ArgumentNullException(nameof(readingOccurrence));
            IsBroken = !(readingOccurrence == ReadingOccurrence.Hourly || readingOccurrence == ReadingOccurrence.Quarterly || readingOccurrence == ReadingOccurrence.Monthly);
            ValidationError = new InvalidMeterReadingOccurrenceRuleError(readingOccurrence.Name);
        }

        public bool IsBroken { get; }

        public ValidationError ValidationError { get; }
    }
}
