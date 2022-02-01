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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.ParentChild;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Common.ChildMeteringPoints
{
    public class ParentCouplingService
    {
        private readonly IGridAreaRepository _gridAreaRepository;

        public ParentCouplingService(IGridAreaRepository gridAreaRepository)
        {
            _gridAreaRepository = gridAreaRepository ?? throw new ArgumentNullException(nameof(gridAreaRepository));
        }

        internal async Task<BusinessRulesValidationResult> CoupleToParentAsync(MeteringPoint child, MeteringPoint parent)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            var childMeteringPoint = CreateChildMeteringPoint(child);
            var parentChildValidation = await childMeteringPoint.CanCoupleToAsync(parent).ConfigureAwait(false);
            if (parentChildValidation.Success == false)
                return BusinessRulesValidationResult.Failure(parentChildValidation.Errors.ToArray());

            await childMeteringPoint.CoupleToAsync(parent).ConfigureAwait(false);
            return BusinessRulesValidationResult.Valid();
        }

        internal void DecoupleFromParent(MeteringPoint child)
        {
            var childMeteringPoint = CreateChildMeteringPoint(child);
            childMeteringPoint.Decouple();
        }

        private ChildMeteringPoint CreateChildMeteringPoint(MeteringPoint child)
        {
            return new ChildMeteringPoint(child, _gridAreaRepository);
        }
    }
}
