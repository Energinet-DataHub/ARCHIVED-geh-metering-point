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

using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Create.Consumption.Validation
{
    public class RuleSet : AbstractValidator<CreateConsumptionMeteringPoint>
    {
        public RuleSet()
        {
            When(request => !string.IsNullOrEmpty(request.ScheduledMeterReadingDate), () =>
            {
                RuleFor(request => request.ScheduledMeterReadingDate)
                    .Must(value => ScheduledMeterReadingDate.CheckRules(value !).Success)
                    .WithState(value => new InvalidScheduledMeterReadingDateRuleError());
            });
        }
    }
}
