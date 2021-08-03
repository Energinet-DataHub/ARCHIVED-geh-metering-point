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
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Energinet.DataHub.MeteringPoints.Infrastructure.PostOffice;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Outbox.ActorMessages
{
    internal class ActorMessageDispatcher : IActorMessageDispatcher
    {
        private readonly IPostOfficeStorageClient _postOfficeStorageClient;

        public ActorMessageDispatcher(
            IPostOfficeStorageClient postOfficeStorageClient)
        {
            _postOfficeStorageClient = postOfficeStorageClient;
        }

        public async Task DispatchMessageAsync(OutboxMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            await _postOfficeStorageClient.WriteAsync(message).ConfigureAwait(false);
        }
    }
}
