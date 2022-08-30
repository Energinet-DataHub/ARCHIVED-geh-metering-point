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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Common
{
    public class CorrelationIdMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private readonly IJsonSerializer _serializer;

        public CorrelationIdMiddleware(
            ILogger<CorrelationIdMiddleware> logger,
            IJsonSerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            var correlationContext = context.GetService<ICorrelationContext>();

            var correlationId = default(string);
            if (context.Is(FunctionExtensions.TriggerType.HttpTrigger))
            {
                correlationId = ParseCorrelationIdFromHeader(context);
            }
            else if (context.Is(FunctionExtensions.TriggerType.ServiceBusTrigger))
            {
                correlationId = ParseCorrelationIdFromMessage(context);
            }

            if (correlationId is null)
            {
                throw new InvalidOperationException($"Could not parse correlation id.");
            }

            _logger.LogInformation($"Correlation id is: {correlationId}");
            correlationContext.SetId(correlationId);

            await next(context).ConfigureAwait(false);
        }

        private static string ParseCorrelationIdFromMessage(FunctionContext context)
        {
            context.BindingContext.BindingData.TryGetValue("CorrelationId", out var correlationObj);

            if (correlationObj is not string correlationId)
            {
                throw new InvalidOperationException($"Could not parse correlation id Service bus message.");
            }

            return correlationId;
        }

        private string ParseCorrelationIdFromHeader(FunctionContext context)
        {
            context.BindingContext.BindingData.TryGetValue("Headers", out var headersObj);

            if (headersObj is not string headersStr)
            {
                throw new InvalidOperationException("Could not read headers");
            }

            var headers = _serializer.Deserialize<Dictionary<string, string>>(headersStr);

            #pragma warning disable CA1308 // Use lower case
            var normalizedKeyHeaders = headers
                .ToDictionary(h => h.Key.ToLowerInvariant(), h => h.Value);
            #pragma warning restore

            normalizedKeyHeaders.TryGetValue("correlationid", out var correlationId);

            if (correlationId is null)
            {
                throw new InvalidOperationException($"Could not parse correlation id from HTTP header.");
            }

            return correlationId;
        }
    }
}
