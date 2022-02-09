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
using Energinet.DataHub.MeteringPoints.Application.UpdateMasterData.Validation;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using FluentValidation;

namespace Energinet.DataHub.MeteringPoints.Application.MarketDocuments
{
    public class ValidationRuleSet : AbstractValidator<MasterDataDocument>
    {
        public ValidationRuleSet()
        {
            When(message => message.ProcessType.Equals(BusinessProcessType.CreateMeteringPoint.Name, StringComparison.OrdinalIgnoreCase), () =>
            {
                RuleFor(request => request).SetValidator(new MeteringGridAreaValidRule());
                RuleFor(request => request).SetValidator(new PhysicalStateRule());
            });

            When(message => message.ProcessType.Equals(BusinessProcessType.ChangeMasterData.Name, StringComparison.OrdinalIgnoreCase), () =>
            {
                RuleFor(request => request.ToGrid)
                    .Null()
                    .WithState(_ => new ToGridAreaIsNotAllowed());
                RuleFor(request => request.FromGrid)
                    .Null()
                    .WithState(_ => new FromGridAreaIsNotAllowed());
            });

            RuleFor(request => request.TransactionId).SetValidator(new TransactionIdValidator());
        }
    }
}
