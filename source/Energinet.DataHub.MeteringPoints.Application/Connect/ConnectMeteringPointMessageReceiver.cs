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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common.Messages;
using Energinet.DataHub.MeteringPoints.Application.MarketDocuments;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.Connect
{
    public class ConnectMeteringPointMessageReceiver : IMessageReceiver
    {
        private readonly IMediator _mediator;
        private readonly IMessageReceiver _next;

        public ConnectMeteringPointMessageReceiver(IMediator mediator, IMessageReceiver next)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _next = next;
        }

        public Task HandleAsync(object message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (message is MasterDataDocument)
            {
                var masterDataDocument = (MasterDataDocument)message;
                var processType = EnumerationType.FromName<BusinessProcessType>(masterDataDocument.ProcessType);
                if (processType == BusinessProcessType.ConnectMeteringPoint)
                {
                    return _mediator.Send(message);
                }
                else
                {
                    return _next?.HandleAsync(message)!;
                }
            }
            else
            {
                return _next?.HandleAsync(message)!;
            }
        }
    }
}
