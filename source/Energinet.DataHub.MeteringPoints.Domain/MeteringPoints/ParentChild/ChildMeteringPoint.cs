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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.ParentChild.Exceptions;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.ParentChild.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.ParentChild
{
    public class ChildMeteringPoint
    {
        private readonly MeteringPoint _meteringPoint;
        private readonly IGridAreaRepository _gridAreaRepository;

        public ChildMeteringPoint(MeteringPoint meteringPoint, IGridAreaRepository gridAreaRepository)
        {
            _meteringPoint = meteringPoint ?? throw new ArgumentNullException(nameof(meteringPoint));
            _gridAreaRepository = gridAreaRepository ?? throw new ArgumentNullException(nameof(gridAreaRepository));
        }

        public async Task<BusinessRulesValidationResult> CanCoupleToAsync(MeteringPoint parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            var errors = new List<ValidationError>();
            if (await GridAreasMatchAsync(parent).ConfigureAwait(false) == false)
            {
                errors.Add(new ParentAndChildGridAreasMustBeTheSame());
            }

            var rules = new List<IBusinessRule>()
            {
                new OnlySpecificGroupsCanActAsParentRule(parent),
                new OnlyGroup3And4CanActAsChildOfGroup1(parent, _meteringPoint),
                new OnlySpecificGroupsCanActAsChildOfGroup2(parent, _meteringPoint),
                new CannotCoupleToClosedDownParentRule(parent),
            };

            errors.AddRange(rules.Where(rule => rule.IsBroken).Select(rule => rule.ValidationError).ToList());

            return new BusinessRulesValidationResult(errors);
        }

        public async Task CoupleToAsync(MeteringPoint parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if ((await CanCoupleToAsync(parent).ConfigureAwait(false)).Success == false)
            {
                throw new ParentCouplingException();
            }

            _meteringPoint.SetParent(parent.Id);
        }

        public void Decouple()
        {
            _meteringPoint.RemoveParent();
        }

        private async Task<bool> GridAreasMatchAsync(MeteringPoint parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            var parentGridArea = await GetGridAreaForAsync(parent).ConfigureAwait(false);
            var childGridArea = await GetGridAreaForAsync(_meteringPoint).ConfigureAwait(false);

            return parentGridArea.Equals(childGridArea);
        }

        private async Task<GridArea> GetGridAreaForAsync(MeteringPoint meteringPoint)
        {
            var gridArea = await _gridAreaRepository.GetByLinkIdAsync(meteringPoint.GridAreaLinkId).ConfigureAwait(false);
            if (gridArea is null)
            {
                throw new ParentCouplingException($"Could not find grid area with link id {meteringPoint.GridAreaLinkId.Value}.");
            }

            return gridArea;
        }
    }
}
