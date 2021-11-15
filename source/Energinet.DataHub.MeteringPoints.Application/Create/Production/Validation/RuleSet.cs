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

using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints.Rules;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Create.Production.Validation
{
    public class RuleSet : AbstractValidator<CreateProductionMeteringPoint>
    {
        public RuleSet()
        {
            RuleFor(request => request.MeteringMethod)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithState(value => new MeteringMethodIsMandatoryValidationError())
                .SetValidator(new MeteringMethodMustBeValidRule());
            RuleFor(request => request.GsrnNumber).SetValidator(new GsrnNumberValidator());
            RuleFor(request => request.DisconnectionType).SetValidator(new DisconnectionTypeRule())
                .Unless(request => string.IsNullOrWhiteSpace(request.DisconnectionType));
            RuleFor(request => request.ConnectionType)
                .SetValidator(new ConnectionTypeRule())
                .Unless(request => string.IsNullOrWhiteSpace(request.ConnectionType));
            RuleFor(request => request.NetSettlementGroup)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithState(createMeteringPoint => new NetSettlementGroupMandatoryValidationError());
            RuleFor(request => request.MeterReadingOccurrence)
                .NotEmpty()
                .WithState(createMeteringPoint => new MeterReadingOccurenceMandatoryValidationError());
            RuleFor(createMeteringPoint => createMeteringPoint.DisconnectionType)
                .NotEmpty()
                .WithState(createMeteringPoint => new DisconnectionTypeMandatoryValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.DisconnectionType));
            RuleFor(createMeteringPoint => createMeteringPoint.PowerPlant)
                .NotEmpty()
                .WithState(createMeteringPoint => new PowerPlantValidationError(createMeteringPoint.GsrnNumber, createMeteringPoint.PowerPlant));
        }
    }
}
