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
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Outbox
{
    public class ActorMessageDispatcher
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOutboxManager _outbox;
        private readonly IPostOfficeStorageClient _postOfficeStorageClient;

        public ActorMessageDispatcher(
            IUnitOfWork unitOfWork,
            IOutboxManager outbox,
            IPostOfficeStorageClient postOfficeStorageClient)
        {
            _unitOfWork = unitOfWork;
            _outbox = outbox;
            _postOfficeStorageClient = postOfficeStorageClient;
        }

        [Function("ActorMessageDispatcher")]
        public async Task RunAsync(
            [TimerTrigger("%ACTOR_MESSAGE_DISPATCH_TRIGGER_TIMER%")] string timerInformation)
        {
            while (true)
            {
                var message = _outbox.GetNext(OutboxMessageCategory.ActorMessage);
                if (message == null)
                {
                    break;
                }

                await _postOfficeStorageClient.WriteAsync(message.Correlation, message.Data).ConfigureAwait(false);
                _outbox.MarkProcessed(message);

                await _unitOfWork.CommitAsync().ConfigureAwait(false);
            }
        }
    }
}
