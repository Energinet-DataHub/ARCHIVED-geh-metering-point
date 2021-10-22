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

using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Validation.Rules
{
    internal class FromGridAreaMustExistRule : IBusinessRule
    {
        private readonly GridArea? _gridArea;

        public FromGridAreaMustExistRule(GridArea? gridArea)
        {
            _gridArea = gridArea;
        }

        public bool IsBroken => _gridArea?.Id == null;

        public ValidationError ValidationError => new FromGridAreaMustExistRuleError(_gridArea);
    }
}
