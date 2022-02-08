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
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.CloseDown
{
    public class CloseDownMeteringPointHandler : ICommandHandler<CloseDownMeteringPoint>
    {
        private readonly IMeteringPointRepository _meteringPoints;

        public CloseDownMeteringPointHandler(IMeteringPointRepository meteringPoints)
        {
            _meteringPoints = meteringPoints ?? throw new ArgumentNullException(nameof(meteringPoints));
        }

        public async Task<Unit> Handle(CloseDownMeteringPoint request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var meteringPoint = await _meteringPoints.GetByGsrnNumberAsync(GsrnNumber.Create(request.GsrnNumber))
                .ConfigureAwait(false);
            meteringPoint?.CloseDown();
            return Unit.Value;
        }
    }
}
