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

using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Create.Exchange.Validation
{
    public class RuleSet : AbstractValidator<CreateExchangeMeteringPoint>
    {
        public RuleSet()
        {
            RuleFor(request => request.MeterNumber)
                .SetValidator(new MeterNumberMustBeValidRule()!)
                .Unless(request => string.IsNullOrEmpty(request.MeterNumber));
            RuleFor(request => request.MeteringMethod)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithState(value => new MeteringMethodIsMandatoryValidationError())
                .SetValidator(new MeteringMethodMustBeValidRule());
            RuleFor(request => request.GsrnNumber).SetValidator(new GsrnNumberValidator());
            RuleFor(request => request.MeterReadingOccurrence)
                    .NotEmpty()
                    .WithState(createMeteringPoint => new MeterReadingOccurenceMandatoryValidationError());
        }
    }
}
