﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Globalization;

namespace Energinet.DataHub.MeteringPoints.Application.InputValidation.Validators
{
    public class OccurenceDateInputValidation : IValidator<CreateMeteringPoint, CreateMeteringPointResult>
    {
        public InputValidationResult Validate(CreateMeteringPoint command)
        {
            if (string.IsNullOrWhiteSpace(command.OccurenceDate))
            {
                return InputValidationResult.Error(nameof(command.OccurenceDate), "Field is mandatory");
            }

            if (!IsValidOccurenceDate(command.OccurenceDate))
            {
                return InputValidationResult.Error(nameof(command.OccurenceDate), "Invalid date format");
            }

            return InputValidationResult.Ok();
        }

        private static bool IsValidOccurenceDate(string value)
        {
            return DateTime.TryParseExact(value, "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }
    }
}
