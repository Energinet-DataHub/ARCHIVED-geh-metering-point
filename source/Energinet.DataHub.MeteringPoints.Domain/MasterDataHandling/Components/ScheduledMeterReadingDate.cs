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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components
{
    public class ScheduledMeterReadingDate : ValueObject
    {
        private ScheduledMeterReadingDate(string monthAndDay)
        {
            MonthAndDay = monthAndDay;
            Day = monthAndDay.Substring(2, 2);
        }

        public string MonthAndDay { get; }

        public string Day { get; private set; }

        public static ScheduledMeterReadingDate Create(string monthAndDay)
        {
            if (monthAndDay == null) throw new ArgumentNullException(nameof(monthAndDay));
            if (CheckRules(monthAndDay).Success == false)
            {
                throw new InvalidScheduledMeterReadingDateException();
            }

            return new ScheduledMeterReadingDate(monthAndDay);
        }

        public static BusinessRulesValidationResult CheckRules(string monthAndDay)
        {
            var rules = new Collection<IBusinessRule>()
            {
                new ScheduledMeterReadingDateFormatRule(monthAndDay),
            };
            return new BusinessRulesValidationResult(rules);
        }
    }
}
