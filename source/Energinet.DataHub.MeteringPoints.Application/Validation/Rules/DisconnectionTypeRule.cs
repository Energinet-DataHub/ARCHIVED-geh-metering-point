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

using System.Collections.Generic;
using System.Data;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class DisconnectionTypeRule : AbstractValidator<CreateMeteringPoint>
    {
        public DisconnectionTypeRule()
        {
            When(MandatoryMeteringPointTypes, () =>
            {
                RuleFor(createMeteringPoint => createMeteringPoint.DisconnectionType)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithState(createMeteringPoint => new DisconnectionTypeMandatoryValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.DisconnectionType))
                    .Must(AllowedDisconnectionTypes)
                    .WithState(createMeteringPoint => new DisconnectionTypeWrongValueValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.DisconnectionType));
            }).Otherwise(DisconnectionTypeMustBeEmpty);
        }

        private static bool AllowedDisconnectionTypes(string disconnectionType)
        {
            return new HashSet<string>
            {
                DisconnectionType.Manual.Name,
                DisconnectionType.Remote.Name,
            }.Contains(disconnectionType);
        }

        private static bool MandatoryMeteringPointTypes(CreateMeteringPoint createMeteringPoint)
        {
            return new HashSet<string>
            {
                MeteringPointType.Production.Name,
                MeteringPointType.Consumption.Name,
            }.Contains(createMeteringPoint.TypeOfMeteringPoint);
        }

        private void DisconnectionTypeMustBeEmpty()
        {
            RuleFor(createMeteringPoint => createMeteringPoint.DisconnectionType)
                .Empty()
                .WithState(createMeteringPoint => new DisconnectionTypeMandatoryValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.DisconnectionType));
        }
    }
}
