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
using Energinet.DataHub.Charges.Clients.DefaultChargeLink;
using Energinet.DataHub.Charges.Clients.Models;
using Energinet.DataHub.MeteringPoints.Application.Integrations.ChargeLinks.Create;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing.Functions
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
        public async Task RunAsync([ServiceBusTrigger("%CHARGES_DEFAULT_LINK_RESPONSE_QUEUE%", Connection = "INTEGRATION_EVENT_QUEUE_CONNECTION")] byte[] data, FunctionContext context)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (context == null) throw new ArgumentNullException(nameof(context));
            _logger.LogInformation($"Received an response from Charges.");

            var reader = new DefaultChargeLinkReplyReader(HandleSuccessAsync, HandleFailureAsync);
            await reader.ReadAsync(data).ConfigureAwait(false);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        private static string CreateSuccessMessage(DefaultChargeLinksCreatedSuccessfullyDto createDefaultChargeLinksSucceeded)
        {
            const string message = "Add default Charge Link request was successful.";
            const string noChargeLinksWereCreated = " No charge links were created";

            return createDefaultChargeLinksSucceeded.DidCreateChargeLinks ? message : $"{message}{noChargeLinksWereCreated}";
        }

        private async Task HandleFailureAsync(DefaultChargeLinksCreationFailedStatusDto createDefaultChargeLinksFailed)
        {
            // TODO: Implement error handling
            _logger.LogInformation($"Add default Charge Links has failed.");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        private async Task HandleSuccessAsync(DefaultChargeLinksCreatedSuccessfullyDto createDefaultChargeLinksSucceeded)
        {
            _logger.LogInformation(CreateSuccessMessage(createDefaultChargeLinksSucceeded));
            CreateDefaultChargeLinksSucceeded notification = new(createDefaultChargeLinksSucceeded.MeteringPointId, createDefaultChargeLinksSucceeded.DidCreateChargeLinks, _correlationContext.Id);
            await _mediator.Publish(notification).ConfigureAwait(false);
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
