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
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Application.GridAreas.Create
{
    public class CreateGridAreaHandler : IBusinessRequestHandler<CreateGridArea>
    {
        private IGridAreaRepository _gridAreaRepository;

        public CreateGridAreaHandler(
            IGridAreaRepository gridAreaRepository)
        {
            _gridAreaRepository = gridAreaRepository;
        }

        public async Task<BusinessProcessResult> Handle(CreateGridArea request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var validationResult = await ValidateInputAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            var gridAreaDetails = CreateDetails(request);

            var businessRulesValidationResult = CheckBusinessRules(request, gridAreaDetails);
            if (!businessRulesValidationResult.Success)
            {
                return businessRulesValidationResult;
            }

            var gridArea = GridArea.Create(gridAreaDetails);

            _gridAreaRepository.Add(gridArea);

            return BusinessProcessResult.Ok(request.TransactionId);
        }

        private static BusinessProcessResult CheckBusinessRules(CreateGridArea request, GridAreaDetails gridAreaDetails)
        {
            var validationResult = GridArea.CanCreate(gridAreaDetails);

            return new BusinessProcessResult(request.TransactionId, validationResult.Errors);
        }

        private static async Task<BusinessProcessResult> ValidateInputAsync(CreateGridArea request, CancellationToken cancellationToken)
        {
            // TODO: implement
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);

            var validationRules = new List<IBusinessRule>
            {
            };

            return new BusinessProcessResult(request.TransactionId, validationRules);
        }

        private static GridAreaDetails CreateDetails(CreateGridArea request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // TODO: convert to value objects.
            return new GridAreaDetails(
                request.Name!,
                GridAreaCode.Create(request.Code),
                request.OperatorName!,
                request.OperatorId!,
                request.PriceAreaCode!);
        }
    }
}
