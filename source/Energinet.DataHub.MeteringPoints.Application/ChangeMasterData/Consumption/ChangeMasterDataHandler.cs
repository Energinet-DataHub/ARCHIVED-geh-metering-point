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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.Policies;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.ChangeMasterData.Consumption
{
    public class ChangeMasterDataHandler : IBusinessRequestHandler<ChangeMasterDataRequest>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly ISystemDateTimeProvider _systemDateTimeProvider;
        private readonly ChangeMasterDataSettings _settings;

        public ChangeMasterDataHandler(IMeteringPointRepository meteringPointRepository, ISystemDateTimeProvider systemDateTimeProvider, ChangeMasterDataSettings settings)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
            _systemDateTimeProvider = systemDateTimeProvider ?? throw new ArgumentNullException(nameof(systemDateTimeProvider));
            _settings = settings;
        }

        public async Task<BusinessProcessResult> Handle(ChangeMasterDataRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var targetMeteringPoint = await FetchTargetMeteringPointAsync(request).ConfigureAwait(false);
            if (targetMeteringPoint == null)
            {
                return new BusinessProcessResult(request.TransactionId, new List<ValidationError>()
                {
                    new MeteringPointMustBeKnownValidationError(request.GsrnNumber),
                });
            }

            var preCheckResult = targetMeteringPoint.CanBeChanged();
            if (preCheckResult.Success == false)
            {
                return new BusinessProcessResult(request.TransactionId, preCheckResult.Errors);
            }

            var valueTransformer = new MasterDataValueTransformer(request, targetMeteringPoint);
            if (valueTransformer.HasError)
            {
                return new BusinessProcessResult(request.TransactionId, valueTransformer.Errors);
            }

            var updatedValues = valueTransformer.UpdatedValues();

            var policyCheckResult = await CheckPoliciesAsync(updatedValues).ConfigureAwait(false);
            if (policyCheckResult.Success == false)
            {
                return new BusinessProcessResult(request.TransactionId, policyCheckResult.Errors);
            }

            var masterDataUpdater = new MasterDataValueUpdater(updatedValues, targetMeteringPoint);
            if (masterDataUpdater.CanUpdate != true)
            {
                return new BusinessProcessResult(request.TransactionId, masterDataUpdater.Errors);
            }

            masterDataUpdater.Update();
            return BusinessProcessResult.Ok(request.TransactionId);
        }

        private async Task<MeteringPoint?> FetchTargetMeteringPointAsync(ChangeMasterDataRequest request)
        {
            return await _meteringPointRepository
                .GetByGsrnNumberAsync(GsrnNumber.Create(request.GsrnNumber))
                .ConfigureAwait(false);
        }

        private Task<BusinessRulesValidationResult> CheckPoliciesAsync(MasterDataDetails masterDataDetails)
        {
            if (masterDataDetails == null) throw new ArgumentNullException(nameof(masterDataDetails));
            var validationResults = new List<BusinessRulesValidationResult>()
            {
                new EffectiveDatePolicy(_settings.NumberOfDaysEffectiveDateIsAllowedToBeforeToday).Check(_systemDateTimeProvider.Now(), masterDataDetails.EffectiveDate),
            };

            var validationErrors = validationResults.SelectMany(results => results.Errors).ToList();
            return Task.FromResult<BusinessRulesValidationResult>(new BusinessRulesValidationResult(validationErrors));
        }
    }
}
