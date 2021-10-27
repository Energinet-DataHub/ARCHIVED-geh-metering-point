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

using System.Linq;
using Energinet.DataHub.MeteringPoints.Application.Backend.GridAreas.Create.Validation.Errors;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Backend.GridAreas.Create.Validation.Rules
{
    public class PriceAreaCodeRule : AbstractValidator<CreateGridArea>
    {
        public PriceAreaCodeRule()
        {
            RuleFor(request => request.PriceAreaCode)
                .Must(priceAreaCode => EnumerationType.GetAll<PriceAreaCode>().Select(x => x.Name).ToHashSet().Contains(priceAreaCode!))
                .WithState(createMeteringPoint => new PriceAreaCodeRuleError(createMeteringPoint.PriceAreaCode!));
        }
    }
}
