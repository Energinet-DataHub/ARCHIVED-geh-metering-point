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

using System;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.SubPostOffice.Functions
{
    public class RequestBundleQueueSubscriber
    {
        private readonly ILogger _logger;
        private readonly ICorrelationContext _correlationContext;
        private readonly ISubPostOfficeClient _subPostOfficeClient;

        public RequestBundleQueueSubscriber(
            ILogger logger,
            ICorrelationContext correlationContext,
            ISubPostOfficeClient subPostOfficeClient)
        {
            _logger = logger;
            _correlationContext = correlationContext;
            _subPostOfficeClient = subPostOfficeClient;
        }

        [Function("RequestBundleQueueSubscriber")]
        public async Task RunAsync(
            [ServiceBusTrigger("sbq-meteringpoints", Connection = "POSTOFFICE_QUEUE_CONNECTION_STRING", IsSessionsEnabled = true)] byte[] data,
            FunctionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            await _subPostOfficeClient.CreateBundleAsync(data).ConfigureAwait(false);

            _logger.LogInformation("Dequeued with correlation id: {correlationId}", _correlationContext.Id);
        }
    }
}
