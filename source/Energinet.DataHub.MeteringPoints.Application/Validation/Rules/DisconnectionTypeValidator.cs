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
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class DisconnectionTypeValidator : AbstractValidator<CreateMeteringPoint>
    {
        public DisconnectionTypeValidator()
        {
            RuleFor(input => input)
                .Cascade(CascadeMode.Stop)
                .Must(ParentDisconnectionTypeIsNotNullOrEmpty)
                .WithState(input => new DisconnectionTypeMandatoryValidationError(input.GsrnNumber))
                .DependentRules(() =>
                {
                    RuleFor(input => input)
                        .Must(IsValidDisconnectionType)
                        .WithState(input => new InvalidDisconnectionTypeValue(input.DisconnectionType));
                });
        }

        private static bool IsParent(string meteringPointType)
        {
            return meteringPointType.Equals(MeteringPointType.Consumption.Name, StringComparison.OrdinalIgnoreCase)
                   || meteringPointType.Equals(MeteringPointType.Production.Name, StringComparison.OrdinalIgnoreCase)
                   || meteringPointType.Equals(MeteringPointType.Exchange.Name, StringComparison.OrdinalIgnoreCase);
        }

        private bool ParentDisconnectionTypeIsNotNullOrEmpty(CreateMeteringPoint meteringPoint)
        {
            if (meteringPoint == null) throw new ArgumentNullException(nameof(meteringPoint));

            if (IsParent(meteringPoint.MeteringPointType))
            {
                return !string.IsNullOrEmpty(meteringPoint.DisconnectionType);
            }

            return true;
        }

        private bool IsValidDisconnectionType(CreateMeteringPoint meteringPoint)
        {
            if (IsParent(meteringPoint.MeteringPointType))
            {
                return new HashSet<string?> { DisconnectionType.Manual.Name, DisconnectionType.Remote.Name }.Contains(
                    meteringPoint.DisconnectionType);
            }

            return true;
        }
    }
}
