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

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Errors.Converters
{
    public class GeoInfoReferenceIsMandatoryErrorConverter : ErrorConverter<GeoInfoReferenceIsMandatoryValidationError>
    {
        protected override ErrorMessage Convert(GeoInfoReferenceIsMandatoryValidationError validationError)
        {
            if (validationError == null) throw new ArgumentNullException(nameof(validationError));

            return new("E86", $"The format of the metering point DAR reference {validationError.Reference} for metering point {validationError.GsrnNumber} must comply with the generic UUID format");
        }
    }
}
