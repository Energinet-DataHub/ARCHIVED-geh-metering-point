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
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.Enums;
using Energinet.DataHub.Charges.Libraries.Models;
using Energinet.DataHub.MeteringPoints.Application.ChargeLinks.Create;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
{
    public class ChargesResponseReceiver
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly ICorrelationContext _correlationContext;

        public ChargesResponseReceiver(
            ILogger logger,
            IMediator mediator,
            ICorrelationContext correlationContext)
        {
            _logger = logger;
            _mediator = mediator;
            _correlationContext = correlationContext;
        }

        [Function("ChargesResponseReceiver")]
        public async Task RunAsync([ServiceBusTrigger("%CHARGES_RESPONSE_QUEUE%", Connection = "SHARED_INTEGRATION_EVENT_SERVICE_BUS_SENDER_CONNECTION_STRING")] byte[] data, FunctionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _logger.LogInformation($"Received an response from Charges.");

            var reader = new DefaultChargeLinkReplyReader(HandleSuccessAsync, HandleFailureAsync);
            await reader.ReadAsync(data, RequestStatus.Succeeded).ConfigureAwait(false);
        }

        private async Task HandleFailureAsync(CreateDefaultChargeLinksFailedDto createDefaultChargeLinksFailed)
        {
            // TODO: Implement error handling
            _logger.LogInformation($"Add default Charge Links has failed.");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        private async Task HandleSuccessAsync(CreateDefaultChargeLinksSucceededDto createDefaultChargeLinksSucceeded)
        {
            _logger.LogInformation($"Add default Charge Link request was successful.");
            CreateDefaultChargeLinksSucceeded notification = new(createDefaultChargeLinksSucceeded.meteringPointId, createDefaultChargeLinksSucceeded.didCreateChargeLinks, _correlationContext.Id);
            await _mediator.Publish(notification).ConfigureAwait(false);
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
