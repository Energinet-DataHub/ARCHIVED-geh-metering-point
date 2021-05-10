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

namespace Energinet.DataHub.MeteringPoints.Application.InputValidation.Validators
{
    public class GsrnInputValidator : IValidator<CreateMeteringPoint, CreateMeteringPointResult>
    {
        private const int RequiredIdLength = 18;

        public InputValidationResult Validate(CreateMeteringPoint command)
        {
            if (!IsValidGsrnNumber(command.GsrnNumber))
            {
                return InputValidationResult.Error(nameof(command.GsrnNumber), "Length is invalid");
            }

            return InputValidationResult.Ok();
        }

        private static int Parse(string input)
        {
            return int.Parse(input, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        private bool IsValidGsrnNumber(string value)
        {
            return LengthIsValid(value) && StartDigitsAreValid(value) && CheckSumIsValid(value);
        }

        private bool LengthIsValid(string value)
        {
            return value.Length == RequiredIdLength;
        }

        private bool StartDigitsAreValid(string value)
        {
            int startDigits = Parse(value.Substring(0, 2));
            return startDigits == 57;
        }

        private bool CheckSumIsValid(string value)
        {
            int definedChecksumDigit = Parse(value.Substring(value.Length - 1));
            int calculatedChecksum = CalculateChecksum(value);
            return calculatedChecksum == definedChecksumDigit;
        }

        private int CalculateChecksum(string value)
        {
            int sum = 0;
            bool positionIsOdd = true;
            for (int currentPosition = 1; currentPosition < RequiredIdLength; currentPosition++)
            {
                int currentValueAtPosition = Parse(value.Substring(currentPosition - 1, 1));
                if (positionIsOdd)
                {
                    sum = sum + (currentValueAtPosition * 3);
                }
                else
                {
                    sum = sum + (currentValueAtPosition * 1);
                }

                positionIsOdd = !positionIsOdd;
            }

            int equalOrHigherMultipleOf = (int)(Math.Ceiling(sum / 10.0) * 10);

            return equalOrHigherMultipleOf - sum;
        }
    }
}
