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
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.MarketDocuments
{
    public class ValidationRuleSet : AbstractValidator<MasterDataDocument>
    {
        public ValidationRuleSet()
        {
            RuleFor(request => request).SetValidator(new MeteringGridAreaValidRule());
            RuleFor(request => request.EffectiveDate).SetValidator(new EffectiveDateRule());
            RuleFor(request => request).SetValidator(new MeteringPointTypeValidRule());
            RuleFor(request => request.ProductType).SetValidator(new ProductTypeRule());
            RuleFor(request => request).SetValidator(new MeasureUnitTypeRule());
            RuleFor(request => request).SetValidator(new MeterReadingOccurenceRule());
            RuleFor(request => request).SetValidator(new ActualAddressRule());
            RuleFor(request => request).SetValidator(new PowerLimitRule());
            RuleFor(request => request).SetValidator(new PhysicalStateRule());
            RuleFor(request => request.TransactionId).SetValidator(new TransactionIdentificationRule());
        }
    }
}
