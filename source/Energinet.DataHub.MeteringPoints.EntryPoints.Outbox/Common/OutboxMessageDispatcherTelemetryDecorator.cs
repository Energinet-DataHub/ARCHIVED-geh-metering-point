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
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.Common
{
    internal class OutboxMessageDispatcherTelemetryDecorator : IOutboxMessageDispatcher
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger _logger;
        private readonly ICorrelationContext _correlationContext;
        private readonly IOutboxMessageDispatcher _decoratee;

        public OutboxMessageDispatcherTelemetryDecorator(
            TelemetryClient telemetryClient,
            ILogger logger,
            ICorrelationContext correlationContext,
            IOutboxMessageDispatcher decoratee)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
            _correlationContext = correlationContext;
            _decoratee = decoratee;
        }

        public async Task DispatchMessageAsync(OutboxMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var traceContext = TraceContext.Parse(message.Correlation);
            if (!traceContext.IsValid)
            {
                _logger.LogWarning("Could not parse trace context for outbox message with id: {messageId} and correlation: {correlationId}", message.Id, message.Correlation);

                await _decoratee.DispatchMessageAsync(message).ConfigureAwait(false);
                return;
            }

            var operation = _telemetryClient.StartOperation<DependencyTelemetry>("Outbox", traceContext.TraceId, traceContext.ParentId);
            operation.Telemetry.Type = "Function";
            try
            {
                _correlationContext.SetId(traceContext.TraceId);
                _correlationContext.SetParentId(traceContext.ParentId);

                operation.Telemetry.Success = true;

                await _decoratee.DispatchMessageAsync(message).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                operation.Telemetry.Success = false;
                _telemetryClient.TrackException(exception);
                _telemetryClient.Flush();
                throw;
            }
            finally
            {
                _telemetryClient.StopOperation(operation);

                _correlationContext.SetId(string.Empty);
                _correlationContext.SetParentId(string.Empty);
            }
        }
    }
}
