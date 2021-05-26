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
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class PostCodeFormatMustBeValidRule : AbstractValidator<Address>
    {
        private const string PostCodeDkFormatRegEx = @"^([0-9]{4})$";
        private const int MaxPostCodeLength = 10;

        public PostCodeFormatMustBeValidRule()
        {
            When(address => address.CountryCode.Equals("DK"), PostCodeDenmarkFormat).Otherwise(PostCodeFormatMaxLength);
        }

        private void PostCodeDenmarkFormat()
        {
            RuleFor(installationLocationAddress => installationLocationAddress.PostCode)
                .Matches(PostCodeDkFormatRegEx)
                .WithState(installationLocationAddress => new PostCodeWrongFormatValidationError(nameof(installationLocationAddress.PostCode)));
        }

        private void PostCodeFormatMaxLength()
        {
            RuleFor(installationLocationAddress => installationLocationAddress.PostCode)
                .MaximumLength(MaxPostCodeLength)
                .WithState(installationLocationAddress => new PostCodeMaximumLengthValidationError(nameof(installationLocationAddress.PostCode), MaxPostCodeLength));
        }
    }
}
