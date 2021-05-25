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

using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class SettlementMethodMustBeValidRule : AbstractValidator<CreateMeteringPoint>
    {
        private readonly List<string> _allowedDomainValuesForConsumptionAndNetLossCorrection = new() { "NonProfiled", "Flex" };

        public SettlementMethodMustBeValidRule()
        {
            When(point => point.TypeOfMeteringPoint.Equals("Consumption") || point.TypeOfMeteringPoint.Equals("NetLossCorrection"), () =>
            {
                RuleFor(createMeteringPoint => createMeteringPoint.SettlementMethod)
                    .NotEmpty()
                    .WithState(createMeteringPoint => new SettlementMethodRequiredValidationError(createMeteringPoint.TypeOfMeteringPoint));
                RuleFor(createMeteringPoint => createMeteringPoint.SettlementMethod)
                    .Must(settlementMethod => _allowedDomainValuesForConsumptionAndNetLossCorrection.Contains(settlementMethod))
                    .WithState(createMeteringPoint => new SettlementMethodMissingRequiredDomainValuesValidationError(createMeteringPoint.SettlementMethod));
            }).Otherwise(() =>
            {
                RuleFor(createMeteringPoint => createMeteringPoint.SettlementMethod)
                    .Empty()
                    .WithState(createMeteringPoint => new SettlementMethodNotAllowedValidationError(createMeteringPoint.TypeOfMeteringPoint));
            });
        }
    }
}
