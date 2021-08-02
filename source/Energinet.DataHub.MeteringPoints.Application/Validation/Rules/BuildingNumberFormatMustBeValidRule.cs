// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class BuildingNumberFormatMustBeValidRule : AbstractValidator<string>
    {
        private const string BuildingNumberDkFormatRegEx = @"^([1-9][0-9]{0,2}|[A-ZÆØÅ]{3})([A-ZÆØÅ])$";
        private const string BuildingNumberNotDkFormatRegEx = @"^[a-zA-Z0-9]{6}$";
        private readonly string _gsrnNumber;

        public BuildingNumberFormatMustBeValidRule(string gsrnNumber, string buildingNumber)
        {
            _gsrnNumber = gsrnNumber;
            When(address => buildingNumber.Equals("DK", StringComparison.Ordinal), BuildingNumberDenmarkFormat)
                .Otherwise(BuildingNumberNotDenmarkFormat);
        }

        private void BuildingNumberDenmarkFormat()
        {
            RuleFor(buildingNumber => buildingNumber)
                .Matches(BuildingNumberDkFormatRegEx)
                .WithState(buildingNumber => new BuildingNumberWrongFormatValidationError(_gsrnNumber, buildingNumber));
        }

        private void BuildingNumberNotDenmarkFormat()
        {
            RuleFor(postCode => postCode)
                .Matches(BuildingNumberNotDkFormatRegEx)
                .WithState(buildingNumber => new BuildingNumberWrongFormatValidationError(_gsrnNumber, buildingNumber));
        }
    }
}
