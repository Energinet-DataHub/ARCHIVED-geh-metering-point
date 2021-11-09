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
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Application.EDI;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.Connect
{
    public class SendAccountingPointCharacteristicsMessageHandler : ICommandHandler<SendAccountingPointCharacteristicsMessage>
    {
        private readonly IMediator _mediator;
        private readonly IBusinessDocumentFactory _businessDocumentFactory;

        public SendAccountingPointCharacteristicsMessageHandler(
            IMediator mediator,
            IBusinessDocumentFactory businessDocumentFactory)
        {
            _mediator = mediator;
            _businessDocumentFactory = businessDocumentFactory;
        }

        public async Task<Unit> Handle(
            SendAccountingPointCharacteristicsMessage request,
            CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var meteringPointDto = await _mediator
                                       .Send(new MeteringPointByGsrnQuery(request.MeteringPointGsrn), cancellationToken)
                                       .ConfigureAwait(false)
                                   ?? throw new InvalidOperationException("Metering point not found");

            // TODO: Current and future suppliers
            var energySuppliers = new List<EnergySupplierDetails>();

            foreach (var energySupplier in energySuppliers)
            {
                _businessDocumentFactory.CreateAccountingPointCharacteristicsMessage(request.TransactionId, meteringPointDto, energySupplier.GlnNumber);
            }

            return Unit.Value;
        }
    }
}
