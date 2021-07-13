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
    public class PostCodeFormatMustBeValidRule : AbstractValidator<string>
    {
        private const string PostCodeDkFormatRegEx = @"^([0-9]{4})$";
        private const int MaxPostCodeLength = 10;
        private readonly string _gsrnNumber;

        public PostCodeFormatMustBeValidRule(string gsrnNumber, string countryCode)
        {
            _gsrnNumber = gsrnNumber;
            When(address => countryCode.Equals("DK", StringComparison.Ordinal), PostCodeDenmarkFormat)
                .Otherwise(PostCodeFormatMaxLength);
        }

        private void PostCodeDenmarkFormat()
        {
            RuleFor(postCode => postCode)
                .Matches(PostCodeDkFormatRegEx)
                .WithState(postCode => new PostCodeWrongFormatValidationError(_gsrnNumber, postCode));
        }

        private void PostCodeFormatMaxLength()
        {
            RuleFor(postCode => postCode)
                .MaximumLength(MaxPostCodeLength)
                .WithState(postCode => new PostCodeMaximumLengthValidationError(_gsrnNumber, postCode, MaxPostCodeLength));
        }
    }
}
