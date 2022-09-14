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
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing.Functions.MarketParticipant
{
    public static class GridAreaCreated
    {
        [Function("GridAreaCreated")]
        public static Task RunAsync(
            [ServiceBusTrigger(
            "%MARKET_PARTICIPANT_CHANGED_TOPIC_NAME%",
            "%MARKET_PARTICIPANT_CHANGED_GRID_AREA_CREATED_SUBSCRIPTION_NAME%",
            Connection = "SHARED_SERVICE_BUS_LISTEN_CONNECTION_STRING")]
            byte[] data,
            FunctionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return Task.CompletedTask;
        }
    }
}
