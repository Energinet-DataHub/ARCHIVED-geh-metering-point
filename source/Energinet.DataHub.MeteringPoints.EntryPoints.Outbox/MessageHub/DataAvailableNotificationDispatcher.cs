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
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.LocalMessageHub;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.MessageHub
{
    public class DataAvailableNotificationDispatcher : IRequestHandler<DataAvailableNotification>
    {
        private readonly IDataAvailableNotificationSender _dataAvailableNotificationSender;
        private readonly ICorrelationContext _correlationContext;

        public DataAvailableNotificationDispatcher(
            IDataAvailableNotificationSender dataAvailableNotificationSender,
            ICorrelationContext correlationContext)
        {
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
            _correlationContext = correlationContext;
        }

        public async Task<Unit> Handle(DataAvailableNotification request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            await _dataAvailableNotificationSender.SendAsync(
                _correlationContext.Id,
                new DataAvailableNotificationDto(request.Uuid, request.Recipient, request.MessageType, request.Origin, request.SupportsBundling, request.RelativeWeight, request.DocumentType)).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
