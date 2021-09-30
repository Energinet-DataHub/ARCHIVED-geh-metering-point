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
using System.Globalization;
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class CapacityRule : AbstractValidator<CreateMeteringPoint>
    {
        //private const int CapacityMaximumLength = 8;
        public CapacityRule()
        {
            // When(request => string.IsNullOrWhiteSpace(request.PhysicalConnectionCapacity) == false, () =>
            // {
            //     RuleFor(request => request.PhysicalConnectionCapacity)
            // });

            // When(IsProductionOrConsumptionWithNetSettlementGroupNotZero, () =>
            // {
            //     RuleFor(request => request.PhysicalConnectionCapacity)
            //         .Must(capacity => !string.IsNullOrWhiteSpace(capacity))
            //         .WithState(request => new CapacityIsMandatoryValidationError(request.GsrnNumber, request.PhysicalConnectionCapacity));
            // });
            // When(IsNotAllowedType, () =>
            // {
            //     RuleFor(request => request.PhysicalConnectionCapacity)
            //         .Null()
            //         .WithState(request => new CapacityIsNotAllowedValidationError(request.GsrnNumber, request.PhysicalConnectionCapacity));
            // });
            //
            // When(request => request.PhysicalConnectionCapacity?.Length > 0, () =>
            // {
            //     RuleFor(request => request.PhysicalConnectionCapacity)
            //         .Cascade(CascadeMode.Stop)
            //         .MaximumLength(CapacityMaximumLength)
            //         .WithState(request => new CapacityMaximumLengthValidationError(request.GsrnNumber, request.PhysicalConnectionCapacity))
            //         .Must(capacity => capacity.Length <= CapacityMaximumLength && float.TryParse(capacity, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out _))
            //         .WithState(request => new CapacityMaximumLengthValidationError(request.GsrnNumber, request.PhysicalConnectionCapacity));
            // });
        }

        private static bool IsProductionOrConsumptionWithNetSettlementGroupNotZero(CreateMeteringPoint request)
        {
            return request.TypeOfMeteringPoint == MeteringPointType.Production.Name ||
                   (request.TypeOfMeteringPoint == MeteringPointType.Consumption.Name && request.NetSettlementGroup != NetSettlementGroup.Zero.Name);
        }

        private static bool IsNotAllowedType(CreateMeteringPoint request)
        {
            var notAllowedMeteringPointTypes = new HashSet<string>
            {
                MeteringPointType.Exchange.Name,
                MeteringPointType.Analysis.Name,
                MeteringPointType.NetConsumption.Name,
                MeteringPointType.ExchangeReactiveEnergy.Name,
                MeteringPointType.InternalUse.Name,
            };

            return notAllowedMeteringPointTypes.Contains(request.TypeOfMeteringPoint);
        }
    }
}
