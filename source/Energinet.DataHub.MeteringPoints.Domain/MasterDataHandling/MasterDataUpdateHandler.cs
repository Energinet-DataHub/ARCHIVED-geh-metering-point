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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.ParentChild.Rules;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling
{
    public class MasterDataUpdateHandler
    {
        private readonly MasterDataValidator _validator;
        private readonly IMeteringPointRepository _meteringPointRepository;

        public MasterDataUpdateHandler(MasterDataValidator validator, IMeteringPointRepository meteringPointRepository)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
        }

        public async Task<BusinessRulesValidationResult> CanUpdateAsync(MeteringPoint targetMeteringPoint, MasterData updatedMasterData)
        {
            if (targetMeteringPoint == null) throw new ArgumentNullException(nameof(targetMeteringPoint));

            var validationErrors = new List<ValidationError>();

            validationErrors.AddRange(targetMeteringPoint.CanUpdateMasterData(updatedMasterData, _validator).Errors);
            validationErrors.AddRange((await EnsureMeterReadingPeriodicityOfChildMatchParentAsync(targetMeteringPoint, updatedMasterData).ConfigureAwait(false)).Errors);

            return new BusinessRulesValidationResult(validationErrors);
        }

        public void Update(MeteringPoint targetMeteringPoint, MasterData updatedMasterData)
        {
            if (targetMeteringPoint == null) throw new ArgumentNullException(nameof(targetMeteringPoint));
            targetMeteringPoint.UpdateMasterData(updatedMasterData, _validator);
        }

        private async Task<BusinessRulesValidationResult> EnsureMeterReadingPeriodicityOfChildMatchParentAsync(MeteringPoint targetMeteringPoint, MasterData updatedMasterData)
        {
            var validationErrors = new List<ValidationError>();
            if (targetMeteringPoint.MeteringPointType != MeteringPointType.ExchangeReactiveEnergy ||
                targetMeteringPoint.ParentMeteringPointId is null)
                return new BusinessRulesValidationResult(validationErrors);

            var parent = await _meteringPointRepository.GetByIdAsync(targetMeteringPoint.ParentMeteringPointId).ConfigureAwait(false);
            if (parent == null) return new BusinessRulesValidationResult(validationErrors);
            if (parent.MasterData.ReadingOccurrence.Equals(updatedMasterData.ReadingOccurrence) == false)
            {
                validationErrors.Add(new ReadingPeriodicityOfChildDoesNotMatchParent());
            }

            return new BusinessRulesValidationResult(validationErrors);
        }
    }
}
