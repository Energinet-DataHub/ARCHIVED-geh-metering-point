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
using Energinet.DataHub.MeteringPoints.Domain.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.Connect
{
    public class SetEnergySupplierDetailsHandler : ICommandHandler<SetEnergySupplierInfo>
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly ISystemDateTimeProvider _systemDateTimeProvider;

        public SetEnergySupplierDetailsHandler(
            IMeteringPointRepository meteringPointRepository,
            ISystemDateTimeProvider systemDateTimeProvider)
        {
            _meteringPointRepository = meteringPointRepository ?? throw new ArgumentNullException(nameof(meteringPointRepository));
            _systemDateTimeProvider = systemDateTimeProvider;
        }

        public async Task<Unit> Handle(SetEnergySupplierInfo request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var meteringPoint =
                await _meteringPointRepository.GetByGsrnNumberAsync(GsrnNumber.Create(request.MeteringPointGsrn)).ConfigureAwait(false);

            if (meteringPoint is null)
            {
                throw new BusinessOperationException($"Metering point ({request.MeteringPointGsrn}) not found.");
            }

            meteringPoint.SetEnergySupplierDetails(EnergySupplierDetails.Create(request.StartOfSupply));
            return Unit.Value;
        }
    }
}
