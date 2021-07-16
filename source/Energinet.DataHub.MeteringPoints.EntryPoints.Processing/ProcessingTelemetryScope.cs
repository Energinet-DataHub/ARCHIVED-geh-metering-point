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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
{
    public class ProcessingTelemetryScope : IFunctionsWorkerMiddleware
    {
        private readonly TelemetryClient _telemetryClient;

        public ProcessingTelemetryScope(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var operation = _telemetryClient.StartOperation<RequestTelemetry>("Processing", CorrelationIdContext.CorrelationId, CorrelationIdContext.ParentId);
            try
            {
                operation.Telemetry.Success = true;
                await next(context).ConfigureAwait(false);
            }
            catch (Exception)
            {
                operation.Telemetry.Success = false;
                throw;
            }
            finally
            {
                _telemetryClient.StopOperation(operation);
            }
        }
    }
}
