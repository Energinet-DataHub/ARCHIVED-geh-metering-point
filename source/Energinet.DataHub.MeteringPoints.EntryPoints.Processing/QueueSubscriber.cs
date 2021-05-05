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
using System.Text;
using Energinet.DataHub.MeteringPoints.Application;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
{
    public class QueueSubscriber
    {
        private readonly ILogger _logger;
        private readonly ICorrelationContext _correlationContext;

        public QueueSubscriber(
            ILogger logger,
            ICorrelationContext correlationContext)
        {
            _logger = logger;
            _correlationContext = correlationContext;
        }

        [Function("QueueSubscriber")]
        public void Run(
            [ServiceBusTrigger("%METERINGPOINT_QUEUE_TOPIC_NAME%", Connection = "METERINGPOINT_QUEUE_CONNECTION_STRING")] byte[] item,
            FunctionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var content = Encoding.UTF8.GetString(item);

            _logger.LogInformation(content);
            _logger.LogInformation("InvocationId: {invocationId}", context.InvocationId);
            _logger.LogInformation("With correlation id: {correlationId}", _correlationContext.GetCorrelationId());

            var message = $"Output message created at {DateTime.Now}";
        }
    }
}
