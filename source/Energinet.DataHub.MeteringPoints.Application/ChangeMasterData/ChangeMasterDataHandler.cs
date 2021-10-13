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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Connect;
using Energinet.DataHub.MeteringPoints.Application.Validation.Rules;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.ChangeMasterData
{
    public class ChangeMasterDataHandler : IBusinessRequestHandler<ChangeMasterDataRequest>
    {
        private IMeteringPointRepository _meteringPointRepository;

        public ChangeMasterDataHandler(IMeteringPointRepository meteringPointRepository)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
        }

        public async Task<BusinessProcessResult> Handle(ChangeMasterDataRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var meteringPoint = await _meteringPointRepository
                .GetByGsrnNumberAsync(GsrnNumber.Create(request.GsrnNumber))
                .ConfigureAwait(false);

            var validationResult = Validate(request, meteringPoint);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            var masterDataDetails = new MasterDataDetails(request.StreetName);
            var rulesCheckResult = CheckBusinessRules(request, masterDataDetails, meteringPoint!);
            if (!rulesCheckResult.Success)
            {
                return rulesCheckResult;
            }

            meteringPoint!.Change(masterDataDetails);

            return BusinessProcessResult.Ok(request.TransactionId);
        }

        private static BusinessProcessResult Validate(ChangeMasterDataRequest request, MeteringPoint? meteringPoint)
        {
            var validationRules = new List<IBusinessRule>()
            {
                new MeteringPointMustBeKnownRule(meteringPoint, request.GsrnNumber),
            };

            return new BusinessProcessResult(request.TransactionId, validationRules);
        }

        private static BusinessProcessResult CheckBusinessRules(ChangeMasterDataRequest request, MasterDataDetails details, MeteringPoint meteringPoint)
        {
            var validationResult = meteringPoint.CanChange(details);

            return new BusinessProcessResult(request.TransactionId, validationResult.Errors);
        }
    }
}
