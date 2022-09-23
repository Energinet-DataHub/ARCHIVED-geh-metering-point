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
using Energinet.DataHub.EnergySupplying.IntegrationEvents;
using Energinet.DataHub.MeteringPoints.Application.Extensions;
using Energinet.DataHub.MeteringPoints.Application.Integrations.ChargeLinks.Create;
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Notifications;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Energinet.DataHub.MeteringPoints.IntegrationEvents.CreateMeteringPoint;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing.Functions.EventListeners
{
    public class MeteringPointMessageDequeuedListener
    {
        private readonly INotificationReceiver _notificationReceiver;
        private readonly ILogger _logger;

        public MeteringPointMessageDequeuedListener(INotificationReceiver notificationReceiver, ILogger logger)
        {
            _notificationReceiver = notificationReceiver;
            _logger = logger;
        }

        [Function("MeteringPointMessageDequeuedListener")]
        public async Task RunAsync([ServiceBusTrigger("%INTEGRATION_EVENT_TOPIC_NAME%", "%METERING_POINT_MESSAGE_DEQUEUED_EVENT_SUBSCRIPTION_NAME%", Connection = "SHARED_SERVICE_BUS_LISTEN_CONNECTION_STRING")] byte[] data, FunctionContext context)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var messageDequeued = MeteringPointMessageDequeuedIntegrationEvent.Parser.ParseFrom(data);
            _logger.LogInformation($"Metering Point Message dequeued with correlation: {messageDequeued.Correlation}");
            var notication = new CreateDefaultChargeLinksNotification(
                messageDequeued.GsrnNumber,
                messageDequeued.Correlation);
            await _notificationReceiver.PublishAndCommitAsync(notication).ConfigureAwait(false);
        }
    }
}
