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
using Energinet.DataHub.MarketParticipant.Integration.Model.Protobuf;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing.Functions.MarketParticipant
{
    public class ActorCreatedReceiver
    {
        private readonly ICommandScheduler _commandScheduler;

        public ActorCreatedReceiver(ICommandScheduler commandScheduler)
        {
            _commandScheduler = commandScheduler;
        }

        [Function("ActorCreatedReceiver")]
        public async Task RunAsync(
            [ServiceBusTrigger(
            "%MARKET_PARTICIPANT_CHANGED_TOPIC_NAME%",
            "%MARKET_PARTICIPANT_CHANGED_ACTOR_CREATED_SUBSCRIPTION_NAME%",
            Connection = "SHARED_SERVICE_BUS_LISTEN_CONNECTION_STRING")]
            byte[] data,
            FunctionContext context)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var message = ActorCreatedIntegrationEventContract.Parser.ParseFrom(data);
            if (message == null) throw new ArgumentException(nameof(message));

            var command = new Application.MarketParticipants.ActorsCreated.ActorCreated(
                Guid.Parse(message.ActorId),
                message.ActorNumber,
                message.Type);

            await _commandScheduler.EnqueueAsync(command).ConfigureAwait(false);
        }
    }
}
