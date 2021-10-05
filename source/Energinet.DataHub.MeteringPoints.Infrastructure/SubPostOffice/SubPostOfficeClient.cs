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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common.Transport;
using Energinet.DataHub.MeteringPoints.Application.PostOffice;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;
using GreenEnergyHub.PostOffice.Communicator.Dequeue;
using GreenEnergyHub.PostOffice.Communicator.Model;
using GreenEnergyHub.PostOffice.Communicator.Peek;
using GreenEnergyHub.PostOffice.Communicator.Storage;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice
{
    public class SubPostOfficeClient : ISubPostOfficeClient
    {
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IDataBundleResponseSender _dataBundleResponseSender;
        private readonly IDequeueNotificationParser _dequeueNotificationParser;
        private readonly IRequestBundleParser _requestBundleParser;
        private readonly IBundleCreator _bundleCreator;
        private readonly IStorageHandler _postOfficeStorageHandler;
        private readonly IPostOfficeMessageMetadataRepository _postOfficeMessageMetadataRepository;

        public SubPostOfficeClient(
            IStorageHandler postOfficeStorageHandler,
            IPostOfficeMessageMetadataRepository postOfficeMessageMetadataRepository,
            IMessageDispatcher messageDispatcher,
            IDataBundleResponseSender dataBundleResponseSender,
            IDequeueNotificationParser dequeueNotificationParser,
            IRequestBundleParser requestBundleParser,
            IBundleCreator bundleCreator)
        {
            _postOfficeStorageHandler = postOfficeStorageHandler;
            _postOfficeMessageMetadataRepository = postOfficeMessageMetadataRepository;
            _messageDispatcher = messageDispatcher;
            _dataBundleResponseSender = dataBundleResponseSender;
            _dequeueNotificationParser = dequeueNotificationParser;
            _requestBundleParser = requestBundleParser;
            _bundleCreator = bundleCreator;
        }

        public async Task CreateBundleAsync(byte[] request)
        {
            var notificationDto = _requestBundleParser.Parse(request);

            var messages = await _postOfficeMessageMetadataRepository.GetMessagesAsync(notificationDto.DataAvailableNotificationIds.ToArray()).ConfigureAwait(false);

            var bundle = await _bundleCreator.CreateBundleAsync(messages).ConfigureAwait(false);

            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(bundle));

            await _postOfficeStorageHandler.AddStreamToStorageAsync(stream, notificationDto).ConfigureAwait(false);

            await _dataBundleResponseSender.SendAsync(new RequestDataBundleResponseDto(new Uri("http://uriToBundleBlob"), notificationDto.DataAvailableNotificationIds), "sessionId", DomainOrigin.MeteringPoints).ConfigureAwait(false);
        }

        public async Task BundleDequeuedAsync(byte[] notification)
        {
            var notificationDto = _dequeueNotificationParser.Parse(notification);

            var messages = await _postOfficeMessageMetadataRepository.GetMessagesAsync(notificationDto.DataAvailableNotificationIds.ToArray()).ConfigureAwait(false);

            foreach (var message in messages)
            {
                IOutboundMessage messageReceived = new MessageReceived(Correlation: message.Correlation);
                await _messageDispatcher.DispatchAsync(messageReceived).ConfigureAwait(false);
            }
        }
    }
}
