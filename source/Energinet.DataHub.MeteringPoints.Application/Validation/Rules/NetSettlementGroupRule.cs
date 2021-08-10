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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class NetSettlementGroupRule : AbstractValidator<CreateMeteringPoint>
    {
        public NetSettlementGroupRule()
        {
            When(MeteringPointTypeIsProductionOrConsumption, () =>
            {
                RuleFor(request => request.NetSettlementGroup)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithState(createMeteringPoint => new NetSettlementGroupMandatoryValidationError(createMeteringPoint.TypeOfMeteringPoint))
                    .Must(netSettlementGroup => AllowedNetSettlementGroupValues().Contains(netSettlementGroup!))
                    .WithState(createMeteringPoint => new NetSettlementGroupInvalidValueValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.TypeOfMeteringPoint));
            }).Otherwise(() =>
            {
                RuleFor(request => request.NetSettlementGroup)
                    .Empty()
                    .WithState(createMeteringPoint => new NetSettlementGroupNotAllowedValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.TypeOfMeteringPoint));
            });
        }

        private static bool MeteringPointTypeIsProductionOrConsumption(CreateMeteringPoint createMeteringPoint)
        {
            return createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Consumption.Name, StringComparison.Ordinal)
                   || createMeteringPoint.TypeOfMeteringPoint.Equals(MeteringPointType.Production.Name, StringComparison.Ordinal);
        }

        private static HashSet<string> AllowedNetSettlementGroupValues()
        {
            return EnumerationType.GetAll<NetSettlementGroup>().Select(x => x.Name).ToHashSet();
        }
    }
}
