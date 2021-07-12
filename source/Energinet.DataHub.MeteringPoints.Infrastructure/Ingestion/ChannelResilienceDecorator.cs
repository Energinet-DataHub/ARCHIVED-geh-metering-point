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

using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion.Resilience;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion
{
    public class ChannelResilienceDecorator : Channel
    {
        private readonly Channel _channel;
        private readonly IChannelResiliencePolicy _policy;

        public ChannelResilienceDecorator(Channel internalServiceBus, IChannelResiliencePolicy policy)
        {
            _channel = internalServiceBus;
            _policy = policy;
        }

        public override async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await _policy.AsyncPolicy.ExecuteAsync(async () =>
            {
                await _channel.WriteAsync(data, cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
