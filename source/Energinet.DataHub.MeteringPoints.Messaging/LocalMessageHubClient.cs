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
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.LocalMessageHub;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling;

namespace Energinet.DataHub.MeteringPoints.Messaging
{
    public class LocalMessageHubClient : ILocalMessageHubClient
    {
        private readonly INotificationHandler _notificationHandler;
        private readonly IDataBundleResponseSender _dataBundleResponseSender;
        private readonly IDequeueNotificationParser _dequeueNotificationParser;
        private readonly IRequestBundleParser _requestBundleParser;
        private readonly IBundleCreator _bundleCreator;
        private readonly ISystemDateTimeProvider _systemDateTimeProvider;
        private readonly IStorageHandler _storageHandler;
        private readonly IMessageHubMessageRepository _messageHubMessageRepository;

        public LocalMessageHubClient(
            IStorageHandler storageHandler,
            IMessageHubMessageRepository messageHubMessageRepository,
            INotificationHandler notificationHandler,
            IDataBundleResponseSender dataBundleResponseSender,
            IDequeueNotificationParser dequeueNotificationParser,
            IRequestBundleParser requestBundleParser,
            IBundleCreator bundleCreator,
            ISystemDateTimeProvider systemDateTimeProvider)
        {
            _storageHandler = storageHandler;
            _messageHubMessageRepository = messageHubMessageRepository;
            _notificationHandler = notificationHandler;
            _dataBundleResponseSender = dataBundleResponseSender;
            _dequeueNotificationParser = dequeueNotificationParser;
            _requestBundleParser = requestBundleParser;
            _bundleCreator = bundleCreator;
            _systemDateTimeProvider = systemDateTimeProvider;
        }

        public async Task CreateBundleAsync(byte[] request, string sessionId)
        {
            var bundleRequestDto = _requestBundleParser.Parse(request);

            var messages = await _messageHubMessageRepository.GetMessagesAsync(bundleRequestDto.DataAvailableNotificationIds.ToArray()).ConfigureAwait(false);

            var bundle = await _bundleCreator.CreateBundleAsync(messages).ConfigureAwait(false);

            foreach (var message in messages)
            {
                message.AddToBundle(bundleRequestDto.IdempotencyId);
            }

            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(bundle));

            var uri = await _storageHandler.AddStreamToStorageAsync(stream, bundleRequestDto).ConfigureAwait(false);

            // TODO - add notification to Outbox instead of sending immediately
            await _dataBundleResponseSender.SendAsync(new RequestDataBundleResponseDto(uri, bundleRequestDto.DataAvailableNotificationIds), sessionId, DomainOrigin.MeteringPoints).ConfigureAwait(false);
        }

        public async Task BundleDequeuedAsync(byte[] notification)
        {
            var dequeueNotificationDto = _dequeueNotificationParser.Parse(notification);

            var messages = await _messageHubMessageRepository.GetMessagesAsync(dequeueNotificationDto.DataAvailableNotificationIds.ToArray()).ConfigureAwait(false);

            foreach (var message in messages)
            {
                message.Dequeue(_systemDateTimeProvider.Now());
                _notificationHandler.Handle(message);
            }
        }
    }
}
