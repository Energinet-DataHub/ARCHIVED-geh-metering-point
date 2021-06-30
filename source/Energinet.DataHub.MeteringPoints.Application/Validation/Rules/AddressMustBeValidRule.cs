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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class AddressMustBeValidRule : AbstractValidator<CreateMeteringPoint>
    {
        public AddressMustBeValidRule()
        {
            When(MeteringPointTypeIsProductionOrConsumption, () =>
            {
                RuleFor(request => request.StreetName).SetValidator(request => new StreetNameMandatoryForMeteringPointTypeMustBeValidRule(request.GsrnNumber));
                RuleFor(request => request.PostCode).SetValidator(request => new PostCodeMandatoryForMeteringPointTypeMustBeValidRule(request.GsrnNumber));
                RuleFor(request => request.CityName).SetValidator(request => new CityNameMandatoryForMeteringPointTypeMustBeValidRule(request.GsrnNumber));
            });

            RuleFor(request => request.PostCode).SetValidator(request => new PostCodeFormatMustBeValidRule(request.GsrnNumber, request.CountryCode));
            RuleFor(request => request.StreetName).SetValidator(request => new StreetNameMaximumLengthMustBeValidRule(request.GsrnNumber));
            RuleFor(request => request.CityName).SetValidator(request => new CityNameMaximumLengthMustBeValidRule(request.GsrnNumber));
        }

        private static bool MeteringPointTypeIsProductionOrConsumption(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Consumption.Name) || createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Production.Name);
        }
    }
}
