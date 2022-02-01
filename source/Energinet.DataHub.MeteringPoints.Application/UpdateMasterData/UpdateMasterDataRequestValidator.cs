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

using Energinet.DataHub.MeteringPoints.Application.Create.Validation;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.UpdateMasterData
{
    public class UpdateMasterDataRequestValidator : AbstractValidator<UpdateMasterDataRequest>
    {
        public UpdateMasterDataRequestValidator()
        {
            RuleFor(request => request.GsrnNumber)
                .SetValidator(new GsrnNumberValidator());
            RuleFor(request => request.EffectiveDate)
                .SetValidator(new EffectiveDateRule());
            RuleFor(request => request.TransactionId)
                .SetValidator(new TransactionIdentificationRule());
            RuleFor(request => request.ParentRelatedMeteringPoint)
                .Must(value => GsrnNumber.CheckRules(value!).Success)
                .WithState(request => new InvalidParentGsrnNumber())
                .Unless(request => string.IsNullOrEmpty(request.ParentRelatedMeteringPoint));
        }
    }
}