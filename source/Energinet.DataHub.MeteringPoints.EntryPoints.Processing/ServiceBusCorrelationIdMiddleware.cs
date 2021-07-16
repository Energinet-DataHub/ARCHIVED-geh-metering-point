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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
{
    /// <summary>
    /// Set <see cref="ICorrelationContext"/> via a ServiceBus function context
    /// </summary>
    public sealed class ServiceBusCorrelationIdMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger _logger;
        private readonly ICorrelationContext _correlationContext;

        public ServiceBusCorrelationIdMiddleware(
            ILogger logger,
            ICorrelationContext correlationContext)
        {
            _logger = logger;
            _correlationContext = correlationContext;
        }

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!string.IsNullOrWhiteSpace(context.TraceContext.TraceParent))
            {
                // Example: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00
                var operation = context.TraceContext.TraceParent.Split("-")[1];
                var parent = context.TraceContext.TraceParent.Split("-")[2];
                CorrelationIdContext.SetCorrelationId(operation);
                _correlationContext.SetCorrelationId(operation);
                CorrelationIdContext.SetParentId(parent);
            }
            else
            {
                _logger.LogWarning("CorrelationId not found for invocation: {invocationId}", context.InvocationId);
                throw new InvalidOperationException();
            }

            await next(context).ConfigureAwait(false);
        }
    }
}
