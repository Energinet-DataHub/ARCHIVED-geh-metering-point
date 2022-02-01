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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.ParentChild.Rules;
using Energinet.DataHub.MeteringPoints.Domain.Policies;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses
{
    public class UpdateMasterDataProcess
    {
        private readonly MasterDataValidator _validator;
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly UpdateMasterDataPolicies _policies;

        public UpdateMasterDataProcess(MasterDataValidator validator, IMeteringPointRepository meteringPointRepository, UpdateMasterDataPolicies policies)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
            _policies = policies ?? throw new ArgumentNullException(nameof(policies));
        }

        public async Task<BusinessRulesValidationResult> UpdateAsync(MeteringPoint targetMeteringPoint, MasterDataUpdater builder, Instant today)
        {
            if (targetMeteringPoint == null) throw new ArgumentNullException(nameof(targetMeteringPoint));
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var valueValidation = builder.Validate();
            if (valueValidation.Success == false)
            {
                return valueValidation;
            }

            var updatedMasterData = builder.Build();

            var policyCheckResult = await CheckPoliciesAsync(updatedMasterData, today).ConfigureAwait(false);
            if (policyCheckResult.Success == false)
            {
                return policyCheckResult;
            }

            var validationErrors = new List<ValidationError>();
            validationErrors.AddRange(targetMeteringPoint.CanUpdateMasterData(updatedMasterData, _validator).Errors);
            validationErrors.AddRange((await EnsureMeterReadingPeriodicityOfChildMatchParentAsync(targetMeteringPoint, updatedMasterData).ConfigureAwait(false)).Errors);

            if (validationErrors.Count > 0)
            {
                return BusinessRulesValidationResult.Failure(validationErrors.ToArray());
            }

            targetMeteringPoint.UpdateMasterData(updatedMasterData, _validator);
            return BusinessRulesValidationResult.Valid();
        }

        private Task<BusinessRulesValidationResult> CheckPoliciesAsync(MasterData masterData, Instant today)
        {
            if (masterData == null) throw new ArgumentNullException(nameof(masterData));
            var validationResults = new List<BusinessRulesValidationResult>()
            {
                new EffectiveDatePolicy(_policies.NumberOfDaysEffectiveDateIsAllowedToBeforeToday).Check(today, masterData.EffectiveDate!),
            };

            var validationErrors = validationResults.SelectMany(results => results.Errors).ToList();
            return Task.FromResult<BusinessRulesValidationResult>(new BusinessRulesValidationResult(validationErrors));
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
