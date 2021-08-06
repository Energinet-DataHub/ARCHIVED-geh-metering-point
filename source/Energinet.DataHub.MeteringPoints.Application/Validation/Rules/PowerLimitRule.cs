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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class PowerLimitRule : AbstractValidator<CreateMeteringPoint>
    {
        public PowerLimitRule()
        {
            When(createMeteringPoint => !string.IsNullOrEmpty(createMeteringPoint.ContractedConnectionCapacity), () =>
            {
                RuleFor(request => request.ContractedConnectionCapacity)
                    .Must(IsValidPowerLimit())
                    .WithState(request => new PowerLimitValidationError(request.GsrnNumber, request.ContractedConnectionCapacity));
            });

            When(createMeteringPoint => !string.IsNullOrEmpty(createMeteringPoint.RatedCurrent), () =>
            {
                RuleFor(request => request.RatedCurrent)
                    .Must(IsValidPowerLimit())
                    .WithState(request => new PowerLimitValidationError(request.GsrnNumber, request.RatedCurrent));
            });
        }

        private static Func<string?, bool> IsValidPowerLimit()
        {
            return powerLimit => powerLimit?.Length <= 6 && powerLimit.All(char.IsDigit);
        }
    }
}
