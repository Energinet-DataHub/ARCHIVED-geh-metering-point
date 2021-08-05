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
using System.Collections.Generic;
using System.Globalization;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class OfficialAddressRule : AbstractValidator<CreateMeteringPoint>
    {
        public OfficialAddressRule()
        {
            When(request => IsProductionOrConsumption(request) || request.IsOfficialAddress, () =>
            {
                RuleFor(request => request.GeoInfoReference)
                    .Must(IsValidReference)
                    .WithState(request => new GeoInfoReferenceIsMandatoryValidationError(request.GsrnNumber, request.GeoInfoReference));
            });
        }

        private static bool IsValidReference(string? reference)
        {
            return !string.IsNullOrWhiteSpace(reference) && Guid.TryParseExact(reference, "D", out _);
        }

        private static bool IsProductionOrConsumption(CreateMeteringPoint request)
        {
            return request.TypeOfMeteringPoint == MeteringPointType.Production.Name ||
                   request.TypeOfMeteringPoint == MeteringPointType.Consumption.Name;
        }
    }
}
