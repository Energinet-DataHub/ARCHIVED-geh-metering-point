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

using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation
{
    public class CreateMeteringPointRuleSet : AbstractValidator<CreateMeteringPoint>
    {
        public CreateMeteringPointRuleSet()
        {
            RuleFor(request => request.GsrnNumber).SetValidator(new GsrnNumberMustBeValidRule());
            RuleFor(request => request).SetValidator(new SettlementMethodMustBeValidRule());
            RuleFor(request => request).SetValidator(new MeteringGridAreaValidRule());
            RuleFor(request => request).SetValidator(new OccurenceDateMustBeValidRule());
            RuleFor(request => request).SetValidator(new MeteringPointTypeValidRule());
            RuleFor(request => request).SetValidator(new AddressRule());
            RuleFor(request => request).SetValidator(new MeteringPointSubTypeMustBeValidRule());
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
        }
    }
}
