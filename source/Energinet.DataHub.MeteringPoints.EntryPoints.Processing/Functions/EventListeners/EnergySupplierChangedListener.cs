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
using Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Notifications;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing.Functions.EventListeners
{
    public static class EnergySupplierChangedListener
    {
        [Function("EnergySupplierChangedListener")]
        public static void Run([ServiceBusTrigger("%INTEGRATION_EVENT_RECEIVED_TOPIC_NAME%", "%ENERGY_SUPPLIER_CHANGED_EVENT_SUBSCRIPTION_NAME%", Connection = "SHARED_SERVICE_BUS_LISTEN_CONNECTION_STRING")] byte[] data, FunctionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
        }
    }
}
