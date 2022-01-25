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
using Energinet.DataHub.MeteringPoints.Application.Create;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.ParentChild;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.Common.ChildMeteringPoints
{
    public class ParentChildCoupling
    {
        private readonly IGridAreaRepository _gridAreaRepository;
        private readonly IMeteringPointRepository _meteringPointRepository;

        public ParentChildCoupling(IGridAreaRepository gridAreaRepository, IMeteringPointRepository meteringPointRepository)
        {
            _gridAreaRepository = gridAreaRepository ?? throw new ArgumentNullException(nameof(gridAreaRepository));
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
        }

        internal async Task<BusinessProcessResult> TryCoupleToParentAsync(MeteringPoint child, string parentGsrnNumber, string transactionId)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));

            var childMeteringPoint = new ChildMeteringPoint(child, _gridAreaRepository);
            var parentMeteringPoint = await _meteringPointRepository.GetByGsrnNumberAsync(GsrnNumber.Create(parentGsrnNumber)).ConfigureAwait(false);
            if (parentMeteringPoint is null)
            {
                return BusinessProcessResult.Fail(
                    transactionId,
                    new ValidationError[] { new ParentMeteringPointWasNotFound(), });
            }

            var parentChildValidation = await childMeteringPoint.CanCoupleToAsync(parentMeteringPoint).ConfigureAwait(false);
            if (parentChildValidation.Success == false)
                return BusinessProcessResult.Fail(transactionId, parentChildValidation.Errors.ToArray());

            await childMeteringPoint.CoupleToAsync(parentMeteringPoint).ConfigureAwait(false);
            return BusinessProcessResult.Ok(transactionId);
        }

        internal Task<BusinessProcessResult> DecoupleFromParentAsync(MeteringPoint child, string transactionId)
        {
            var childMeteringPoint = new ChildMeteringPoint(child, _gridAreaRepository);
            childMeteringPoint.Decouple();
            return Task.FromResult(BusinessProcessResult.Ok(transactionId));
        }
    }
}
