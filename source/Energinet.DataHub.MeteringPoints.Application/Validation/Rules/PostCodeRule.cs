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
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class PostCodeRule : AbstractValidator<CreateMeteringPoint>
    {
        private const string PostCodeDkFormatRegEx = @"^([0-9]{4})$";
        private const int MaxPostCodeLength = 10;

        public PostCodeRule()
        {
            When(request => !string.IsNullOrWhiteSpace(request.CountryCode) && request.CountryCode.Equals("DK", StringComparison.Ordinal) && !string.IsNullOrEmpty(request.PostCode), PostCodeDenmarkFormat)
                .Otherwise(PostCodeFormatMaxLength);

            When(CreateMeteringPointRulesHelper.MeteringPointTypeIsProductionOrConsumption, () =>
            {
                RuleFor(request => request.PostCode)
                    .SetValidator(request => new PostCodeMandatoryForMeteringPointTypeMustBeValidRule(request.GsrnNumber));
            });
        }

        private void PostCodeDenmarkFormat()
        {
            RuleFor(request => request.PostCode)
                .Matches(PostCodeDkFormatRegEx)
                .WithState(request => new PostCodeWrongFormatValidationError(request.GsrnNumber, request.PostCode!));
        }

        private void PostCodeFormatMaxLength()
        {
            RuleFor(request => request.PostCode)
                .MaximumLength(MaxPostCodeLength)
                .WithState(request => new PostCodeMaximumLengthValidationError(request.GsrnNumber, request.PostCode!, MaxPostCodeLength));
        }
    }
}
