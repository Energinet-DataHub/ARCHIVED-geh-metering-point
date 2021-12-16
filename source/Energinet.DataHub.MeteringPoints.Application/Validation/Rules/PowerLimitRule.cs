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

using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    public class PowerLimitRule : AbstractValidator<MasterDataDocument>
    {
        public PowerLimitRule()
        {
            RuleFor(request => request.MaximumPower !)
                .Must(kwh => PowerLimit.CheckRules(kwh, 0).Success)
                .WithState(request => new InvalidKwhPowerLimitRuleError(request.MaximumPower));

            RuleFor(request => request.MaximumCurrent)
                .Must(ampere => PowerLimit.CheckRules(0, ampere).Success)
                .WithState(request => new InvalidAmperePowerLimitRuleError(request.MaximumCurrent));
        }
    }
}
