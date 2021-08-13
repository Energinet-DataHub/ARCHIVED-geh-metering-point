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
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class BuildingNumberRule : AbstractValidator<CreateMeteringPoint>
    {
        private const string BuildingNumberDenmarkFormatRegEx = @"^(?=.{1,4}$)((([1-9][0-9]{0,2})?[A-ZÆØÅ]*)|([A-ZÆØÅ]*([1-9][0-9]{0,2})?))$";
        private const int BuildingNumberNotDenmarkFormatMaxLength = 6;

        public BuildingNumberRule()
        {
            When(request => !string.IsNullOrWhiteSpace(request.CountryCode) && request.CountryCode.Equals("DK", StringComparison.OrdinalIgnoreCase), BuildingNumberMustBeDanishFormat)
                .Otherwise(BuildingNumberMustNotBeDanishFormat);
        }

        private void BuildingNumberMustBeDanishFormat()
        {
            RuleFor(request => request.BuildingNumber)
                .Matches(BuildingNumberDenmarkFormatRegEx)
                .WithState(request => new BuildingNumberMustBeValidValidationError(request.GsrnNumber, request.BuildingNumber!));
        }

        private void BuildingNumberMustNotBeDanishFormat()
        {
            RuleFor(request => request.BuildingNumber)
                .MaximumLength(BuildingNumberNotDenmarkFormatMaxLength)
                .WithState(request => new BuildingNumberMustBeValidValidationError(request.GsrnNumber, request.BuildingNumber!));
        }
    }
}
