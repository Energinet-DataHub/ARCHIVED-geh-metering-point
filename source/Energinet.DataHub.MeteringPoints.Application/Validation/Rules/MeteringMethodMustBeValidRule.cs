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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class MeteringMethodMustBeValidRule : AbstractValidator<string>
    {
        public MeteringMethodMustBeValidRule()
        {
            RuleFor(value => value)
                .Must(value => EnumerationType.GetAll<MeteringMethod>().Select(item => item.Name)
                    .Contains(value, StringComparer.OrdinalIgnoreCase))
                .WithState(value => new InvalidMeteringMethodValue(value));
        }
    }
}
