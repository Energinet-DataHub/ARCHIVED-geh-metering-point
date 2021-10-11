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

using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Client.Dequeue;
using Energinet.DataHub.MessageHub.Client.Model;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MeteringPoints.Application.Common.Transport;
using Energinet.DataHub.MeteringPoints.Application.MessageHub;
using Energinet.DataHub.MeteringPoints.Infrastructure.LocalMessageHub.Bundling;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.LocalMessageHub
{
    public class LocalMessageHubClient : ILocalMessageHubClient
    {
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IDataBundleResponseSender _dataBundleResponseSender;
        private readonly IDequeueNotificationParser _dequeueNotificationParser;
        private readonly IRequestBundleParser _requestBundleParser;
        private readonly IBundleCreator _bundleCreator;
        private readonly IStorageHandler _storageHandler;
        private readonly IMessageHubMessageRepository _messageHubMessageRepository;

        public LocalMessageHubClient(
            IStorageHandler storageHandler,
            IMessageHubMessageRepository messageHubMessageRepository,
            IMessageDispatcher messageDispatcher,
            IDataBundleResponseSender dataBundleResponseSender,
            IDequeueNotificationParser dequeueNotificationParser,
            IRequestBundleParser requestBundleParser,
            IBundleCreator bundleCreator)
        {
            _storageHandler = storageHandler;
            _messageHubMessageRepository = messageHubMessageRepository;
            _messageDispatcher = messageDispatcher;
            _dataBundleResponseSender = dataBundleResponseSender;
            _dequeueNotificationParser = dequeueNotificationParser;
            _requestBundleParser = requestBundleParser;
            _bundleCreator = bundleCreator;
        }

        public async Task CreateBundleAsync(byte[] request, string sessionId)
        {
            var notificationDto = _requestBundleParser.Parse(request);

            var messages = await _messageHubMessageRepository.GetMessagesAsync(notificationDto.DataAvailableNotificationIds.ToArray()).ConfigureAwait(false);

            var bundle = await _bundleCreator.CreateBundleAsync(messages).ConfigureAwait(false);

            // TODO: Set BundleId on every message (e.g. notification.IdempotencyId)
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(bundle));

            var uri = await _storageHandler.AddStreamToStorageAsync(stream, notificationDto).ConfigureAwait(false);

            // TODO - add notification to Outbox instead of sending immediately
            await _dataBundleResponseSender.SendAsync(new RequestDataBundleResponseDto(uri, notificationDto.DataAvailableNotificationIds), sessionId, DomainOrigin.MeteringPoints).ConfigureAwait(false);
        }

        public async Task BundleDequeuedAsync(byte[] notification)
        {
            var notificationDto = _dequeueNotificationParser.Parse(notification);

            var messages = await _messageHubMessageRepository.GetMessagesAsync(notificationDto.DataAvailableNotificationIds.ToArray()).ConfigureAwait(false);

            foreach (var message in messages)
            {
                // TODO : update message with date and time for dequeue execution
                // TODO : this must be handled by raising an Integration Event through Outbox
                IOutboundMessage messageReceived = new MessageReceived(Correlation: message.Correlation);
                await _messageDispatcher.DispatchAsync(messageReceived).ConfigureAwait(false);
            }
        }
    }
}
