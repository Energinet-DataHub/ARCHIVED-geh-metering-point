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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Validation.ValidationErrors;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;

namespace Energinet.DataHub.MeteringPoints.Application.CloseDown
{
    public class RequestCloseDownHandler : IBusinessRequestHandler<RequestCloseDown>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IBusinessProcessRepository _businessProcesses;

        public RequestCloseDownHandler(IMeteringPointRepository meteringPointRepository, IBusinessProcessRepository businessProcesses)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
            _businessProcesses = businessProcesses ?? throw new ArgumentNullException(nameof(businessProcesses));
        }

        public async Task<BusinessProcessResult> Handle(RequestCloseDown request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var businessProcess = BusinessProcess.Create(BusinessProcessId.Create(), request.TransactionId, BusinessProcessType.CloseDownMeteringPoint);
            _businessProcesses.Add(businessProcess);

            var targetMeteringPoint = await _meteringPointRepository
                .GetByGsrnNumberAsync(GsrnNumber.Create(request.GsrnNumber))
                .ConfigureAwait(false);
            if (targetMeteringPoint == null)
            {
                return BusinessProcessResult.Fail(request.TransactionId, new MeteringPointMustBeKnownValidationError(request.GsrnNumber));
            }

            businessProcess.RequestWasAccepted();

            return BusinessProcessResult.Ok(request.TransactionId);
        }
    }
}
