﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.LocalMessageHub.Functions
{
    public class RequestBundleQueueSubscriber
    {
        private readonly ILogger _logger;
        private readonly ICorrelationContext _correlationContext;
        private readonly ILocalMessageHubClient _localMessageHubClient;
        private readonly ISessionContext _sessionContext;
        private readonly IUnitOfWork _unitOfWork;

        public RequestBundleQueueSubscriber(
            ILogger logger,
            ICorrelationContext correlationContext,
            ILocalMessageHubClient localMessageHubClient,
            ISessionContext sessionContext,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _correlationContext = correlationContext;
            _localMessageHubClient = localMessageHubClient;
            _sessionContext = sessionContext;
            _unitOfWork = unitOfWork;
        }

        [Function("RequestBundleQueueSubscriber")]
        public async Task RunAsync([ServiceBusTrigger("%REQUEST_BUNDLE_QUEUE_SUBSCRIBER_QUEUE%", Connection = "MESSAGEHUB_QUEUE_CONNECTION_STRING", IsSessionsEnabled = true)] byte[] data)
        {
            await _localMessageHubClient.CreateBundleAsync(data, _sessionContext.Id).ConfigureAwait(false);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);

            _logger.LogInformation("Dequeued with correlation id: {CorrelationId}", _correlationContext.Id);
        }
    }
}
