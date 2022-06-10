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
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Application.EnergySuppliers.Queries;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.Connect
{
    public class
        SendAccountingPointCharacteristicsMessageHandler : ICommandHandler<SendAccountingPointCharacteristicsMessage, Unit>
    {
        private readonly IMediator _mediator;
        private readonly IActorMessageService _actorMessageService;

        public SendAccountingPointCharacteristicsMessageHandler(
            IMediator mediator,
            IActorMessageService actorMessageService)
        {
            _mediator = mediator;
            _actorMessageService = actorMessageService;
        }

        public async Task<Unit> Handle(
            SendAccountingPointCharacteristicsMessage request,
            CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var meteringPointDto = await _mediator
                                       .Send(new MeteringPointByIdQuery(request.MeteringPointId), cancellationToken)
                                       .ConfigureAwait(false)
                                   ?? throw new InvalidOperationException("Metering point not found");

            var energySuppliers = await _mediator
                .Send(new EnergySuppliersByMeteringPointIdQuery(request.MeteringPointId), cancellationToken)
                .ConfigureAwait(false);

            foreach (var energySupplier in energySuppliers)
            {
                await _actorMessageService
                    .SendAccountingPointCharacteristicsMessageAsync(request.TransactionId, request.Reason, meteringPointDto, energySupplier)
                    .ConfigureAwait(false);
            }

            return Unit.Value;
        }
    }
}
