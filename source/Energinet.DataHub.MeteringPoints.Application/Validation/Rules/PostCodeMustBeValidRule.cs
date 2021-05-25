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

using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class PostCodeMustBeValidRule : AbstractValidator<CreateMeteringPoint>
    {
        private const int MaxPostCodeLength = 10;
        private const string PostCodeDkFormatRegEx = @"^([0-9]{4})$";

        public PostCodeMustBeValidRule()
        {
            When(createMeteringPoint => createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Consumption.Name) || createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Production.Name), () =>
            {
                RuleFor_MandatoryForConsumptionAndProduction();
                RuleFor_PostCodeFormat();
            })
            .Otherwise(RuleFor_PostCodeFormat);
        }

        private void RuleFor_PostCodeFormat()
        {
            When(createMeteringPoint => createMeteringPoint.InstallationLocationAddress.CountryCode.Equals("DK"), RuleFor_PostCodeFormatDenmark)
            .Otherwise(RuleFor_PostCodeFormatMaxLength);
        }

        private void RuleFor_PostCodeFormatDenmark()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.InstallationLocationAddress.PostCode)
                .Matches(PostCodeDkFormatRegEx)
                .WithState(createMeteringPoint => new WrongFormatValidationError(nameof(createMeteringPoint.InstallationLocationAddress.PostCode)));
        }

        private void RuleFor_PostCodeFormatMaxLength()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.InstallationLocationAddress.PostCode)
                .MaximumLength(MaxPostCodeLength)
                .WithState(createMeteringPoint => new MaximumLengthValidationError(nameof(createMeteringPoint.InstallationLocationAddress.PostCode), MaxPostCodeLength));
        }

        private void RuleFor_MandatoryForConsumptionAndProduction()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.InstallationLocationAddress.PostCode)
                .NotEmpty()
                .WithState(createMeteringPoint => new MandatoryFieldForMeteringPointTypeValidationError(nameof(createMeteringPoint.InstallationLocationAddress.PostCode), createMeteringPoint.TypeOfMeteringPoint));
        }
    }
}
