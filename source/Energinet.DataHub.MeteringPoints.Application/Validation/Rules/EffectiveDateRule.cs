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

using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class EffectiveDateRule : AbstractValidator<string>
    {
        public EffectiveDateRule()
        {
            EffectiveDateRequired();
            EffectiveDateFormat();
        }

        private void EffectiveDateRequired()
        {
            RuleFor(effectiveDate => effectiveDate)
                .NotEmpty()
                .WithState(createMeteringPoint => new EffectiveDateRequiredValidationError());
        }

        private void EffectiveDateFormat()
        {
            RuleFor(effectiveDate => effectiveDate)
                .Must(effectiveDate => EffectiveDate.CheckRules(effectiveDate).Success)
                .WithState(effectiveDate => new EffectiveDateWrongFormatValidationError(effectiveDate));
        }
    }
}
