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
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Common
{
    #pragma warning disable
    public static class FunctionContextExtensions
    {
        public static bool Is(this FunctionContext context, Trigger trigger)
        {
            return context.FunctionDefinition.InputBindings.Any(input => input.Value.Type == trigger.ToString());
        }
    }

    public enum Trigger
    {
        HttpTrigger,
        TimerTrigger,
        ServiceBusTrigger,
    }

    public class EntryPointTelemetryScopeMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ICorrelationContext _correlationContext;

        public EntryPointTelemetryScopeMiddleware(
            TelemetryClient telemetryClient,
            ICorrelationContext correlationContext)
        {
            _telemetryClient = telemetryClient;
            _correlationContext = correlationContext;
        }

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Is(Trigger.ServiceBusTrigger))
            {
                // ServiceBusTrigger
            }

            if (context.Is(Trigger.TimerTrigger))
            {
                // TimerTrigger
            }

            var operation = _telemetryClient.StartOperation<DependencyTelemetry>(context.FunctionDefinition.Name, _correlationContext.Id, _correlationContext.ParentId);
            operation.Telemetry.Type = "Function";
            try
            {
                operation.Telemetry.Success = true;
                await next(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                operation.Telemetry.Success = false;
                _telemetryClient.TrackException(exception);
                throw;
            }
            finally
            {
                _telemetryClient.StopOperation(operation);
            }
        }
    }
}
