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
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption.Rules;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Create
{
    public class CreateMeteringPointRuleSet : AbstractValidator<CreateMeteringPoint>
    {
        public CreateMeteringPointRuleSet()
        {
            // Rules for Consumption metering points
            When(request => request.TypeOfMeteringPoint.Equals(MeteringPointType.Consumption.Name, StringComparison.OrdinalIgnoreCase), () =>
            {
                When(request => !string.IsNullOrEmpty(request.ScheduledMeterReadingDate), () =>
                {
                    RuleFor(request => request.ScheduledMeterReadingDate)
                        .Must(value => ScheduledMeterReadingDate.CheckRules(value !).Success)
                        .WithState(value => new InvalidScheduledMeterReadingDateRuleError());
                });
            });
            RuleFor(request => request).SetValidator(new ConnectionTypeRule());
            RuleFor(request => request.GsrnNumber).SetValidator(new GsrnNumberMustBeValidRule());
            RuleFor(request => request).SetValidator(new SettlementMethodMustBeValidRule());
            RuleFor(request => request).SetValidator(new MeteringGridAreaValidRule());
            RuleFor(request => request.EffectiveDate).SetValidator(new EffectiveDateRule());
            RuleFor(request => request).SetValidator(new MeteringPointTypeValidRule());
            RuleFor(request => request).SetValidator(new MeteringMethodMustBeValidRule());
            RuleFor(request => request).SetValidator(new MeterNumberMustBeValidRule());
            RuleFor(request => request).SetValidator(new NetSettlementGroupRule());
            RuleFor(request => request).SetValidator(new ProductTypeRule());
            RuleFor(request => request).SetValidator(new MeasureUnitTypeRule());
            RuleFor(request => request).SetValidator(new MeterReadingOccurenceRule());
            RuleFor(request => request).SetValidator(new CapacityRule());
            RuleFor(request => request).SetValidator(new LocationDescriptionMustBeValidRule());
            RuleFor(request => request).SetValidator(new PowerPlantMustBeValidRule());
            RuleFor(request => request).SetValidator(new OfficialAddressRule());
            RuleFor(request => request).SetValidator(new AssetTypeRule());
            RuleFor(request => request).SetValidator(new PowerLimitRule());
            RuleFor(request => request).SetValidator(new PhysicalStateRule());
            RuleFor(request => request.TransactionId).SetValidator(new TransactionIdentificationRule());
            RuleFor(request => request.CountryCode).SetValidator(new CountryCodeRule());
        }
    }
}
