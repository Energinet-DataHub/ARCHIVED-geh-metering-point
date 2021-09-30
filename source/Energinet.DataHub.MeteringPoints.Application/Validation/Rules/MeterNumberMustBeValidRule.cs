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
using System.Runtime.InteropServices.ComTypes;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;
using FluentValidation.Results;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class MeterNumberMustBeValidRule : AbstractValidator<CreateMeteringPoint>
    {
        private BusinessRulesValidationResult? _result;

        public MeterNumberMustBeValidRule()
        {
            When(request => !string.IsNullOrWhiteSpace(request.MeterNumber), () =>
            {
                RuleFor(request => request.MeterNumber)
                    .Must(meterId =>
                    {
                        _result = MeterId.CheckRules(meterId!);
                        return _result.Success;
                    })
                    .WithState(request => _result!.Errors.First());
            });
        }
    }
}
