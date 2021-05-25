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
    public class CityNameMustBeValidRule : AbstractValidator<CreateMeteringPoint>
    {
        private const int MaxCityNameLength = 25;

        public CityNameMustBeValidRule()
        {
            When(createMeteringPoint => createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Consumption.Name) || createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Production.Name), () =>
            {
                RuleFor_MandatoryForConsumptionAndProduction();
                RuleFor_CityNameMaximumLength();
            })
            .Otherwise(RuleFor_CityNameMaximumLength);
        }

        private void RuleFor_MandatoryForConsumptionAndProduction()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.InstallationLocationAddress.CityName)
                .NotEmpty()
                .WithState(createMeteringPoint => new MandatoryFieldForMeteringPointTypeValidationError(nameof(createMeteringPoint.InstallationLocationAddress.CityName), createMeteringPoint.TypeOfMeteringPoint));
        }

        private void RuleFor_CityNameMaximumLength()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.InstallationLocationAddress.CityName)
                .MaximumLength(MaxCityNameLength)
                .WithState(createMeteringPoint => new MaximumLengthValidationError(nameof(createMeteringPoint.InstallationLocationAddress.CityName), MaxCityNameLength));
        }
    }
}
