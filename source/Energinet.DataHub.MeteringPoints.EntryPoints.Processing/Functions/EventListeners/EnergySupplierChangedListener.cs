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
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Notifications;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing.Functions.EventListeners
{
    public class EnergySupplierChangedListener
    {
        private readonly INotificationReceiver _notificationReceiver;
        private readonly ILogger _logger;

        public EnergySupplierChangedListener(INotificationReceiver notificationReceiver, ILogger logger)
        {
            _notificationReceiver = notificationReceiver;
            _logger = logger;
        }

        [Function("EnergySupplierChangedListener")]
        public async Task RunAsync([ServiceBusTrigger("%INTEGRATION_EVENT_RECEIVED_TOPIC_NAME%", "%ENERGY_SUPPLIER_CHANGED_EVENT_SUBSCRIPTION_NAME%", Connection = "SHARED_SERVICE_BUS_LISTEN_CONNECTION_STRING")] byte[] data, FunctionContext context)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var energySupplierChangedEvent = EnergySupplierChanged.Parser.ParseFrom(data);

            _logger.LogInformation(@$"Energy supplier changed event recevied with energy supplier gln:
             {energySupplierChangedEvent.EnergySupplierGln} on the following metering point (gsrn):
             {energySupplierChangedEvent.GsrnNumber}");

            var energySupplierChangedNotification = new Application.Connect.EnergySupplierChanged()
            {
                MeteringPointId = energySupplierChangedEvent.AccountingpointId,
                GsrnNumber = energySupplierChangedEvent.GsrnNumber,
                StartOfSupply = energySupplierChangedEvent.EffectiveDate.ToInstant(),
                GlnNumber = energySupplierChangedEvent.EnergySupplierGln,
            };

            await _notificationReceiver.PublishAndCommitAsync(energySupplierChangedNotification).ConfigureAwait(false);
        }
    }
}
