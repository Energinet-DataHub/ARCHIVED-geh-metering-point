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
using System.Linq;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Consumption.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Production;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Create.Validation
{
    public class RuleSet : AbstractValidator<CreateMeteringPoint>
    {
        public RuleSet()
        {
            RuleFor(request => request.MeteringPointType)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithState(request => new MeteringPointTypeRequiredValidationError())
                .Must(value => EnumerationType.GetAll<MeteringPointType>().Select(item => item.Name).Contains(value, StringComparer.OrdinalIgnoreCase))
                .WithState(request =>
                    new MeteringPointTypeValidationError(request.MeteringPointType ?? string.Empty));
            RuleFor(request => request.EffectiveDate).SetValidator(new EffectiveDateValidator());
            RuleFor(request => request.CountryCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithState(createMeteringPoint =>
                    new CountryCodeMandatoryValidationError(createMeteringPoint.GsrnNumber))
                .SetValidator(new CountryCodeRule());
            RuleFor(request => request.AssetType)
                .SetValidator(new AssetTypeRule()!)
                .Unless(request => string.IsNullOrWhiteSpace(request.AssetType));
            RuleFor(createMeteringPoint => createMeteringPoint.PowerPlant)
                .SetValidator(new PowerPlantMustBeValidRule()!)
                .Unless(request => string.IsNullOrWhiteSpace(request.PowerPlant));
            RuleFor(request => request.PhysicalConnectionCapacity)
                .SetValidator(new CapacityRule()!)
                .Unless(request => string.IsNullOrWhiteSpace(request.PhysicalConnectionCapacity));
            RuleFor(request => request.MeterNumber)
                .SetValidator(new MeterNumberMustBeValidRule()!)
                .Unless(request => string.IsNullOrEmpty(request.MeterNumber));
            RuleFor(request => request.MeteringMethod)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithState(value => new MeteringMethodIsMandatoryValidationError())
                .SetValidator(new MeteringMethodMustBeValidRule());
            RuleFor(request => request.GsrnNumber).SetValidator(new GsrnNumberValidator());
            RuleFor(request => request.ConnectionType).SetValidator(new ConnectionTypeRule())
                .Unless(request => string.IsNullOrWhiteSpace(request.ConnectionType));

            When(request => !string.IsNullOrEmpty(request.ScheduledMeterReadingDate), () =>
            {
                RuleFor(request => request.ScheduledMeterReadingDate)
                    .Must(value => ScheduledMeterReadingDate.CheckRules(value !).Success)
                    .WithState(value => new InvalidScheduledMeterReadingDateRuleError());
            });
            RuleFor(request => request.NetSettlementGroup)
                .Must(value => EnumerationType.GetAll<NetSettlementGroup>()
                    .Select(item => item.Name)
                    .Contains(value, StringComparer.OrdinalIgnoreCase))
                .WithState(createMeteringPoint => new InvalidNetSettlementGroupValue())
                .Unless(request => string.IsNullOrEmpty(request.NetSettlementGroup));

            RuleFor(request => request.MeterReadingOccurrence)
                .NotEmpty()
                .WithState(createMeteringPoint => new MeterReadingPeriodicityIsRequired());
            RuleFor(createMeteringPoint => createMeteringPoint.DisconnectionType)
                .Cascade(CascadeMode.Stop)
                .SetValidator(createMeteringPoint => new DisconnectionTypeValidator());
            RuleFor(request => request.ParentRelatedMeteringPoint)
                .Must(value => GsrnNumber.CheckRules(value!).Success)
                .WithState(request => new InvalidParentGsrnNumber())
                .Unless(request => string.IsNullOrEmpty(request.ParentRelatedMeteringPoint));
        }
    }
}
