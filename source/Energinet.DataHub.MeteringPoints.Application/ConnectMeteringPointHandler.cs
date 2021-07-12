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
using Energinet.DataHub.MeteringPoints.Application.Extensions;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application
{
    public class ConnectMeteringPointHandler : IRequestHandler<ConnectMeteringPoint, BusinessProcessResult>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;

        public ConnectMeteringPointHandler(IMeteringPointRepository meteringPointRepository)
        {
            _meteringPointRepository = meteringPointRepository ??
                                       throw new ArgumentNullException(nameof(meteringPointRepository));
        }

        public async Task<BusinessProcessResult> Handle(ConnectMeteringPoint request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var meteringPoint = await _meteringPointRepository
                .GetByGsrnNumberAsync(GsrnNumber.Create(request.GsrnNumber))
                .ConfigureAwait(false);

            if (meteringPoint is null)
            {
                return BusinessProcessResult.Fail(request.TransactionId);
            }

            meteringPoint.Connect(effectiveDate: request.EffectiveDate.ToInstant());

            return BusinessProcessResult.Ok(request.TransactionId);
        }
    }
}
