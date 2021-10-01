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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common.Transport;
using Energinet.DataHub.MeteringPoints.Application.PostOffice;
using Energinet.DataHub.MeteringPoints.Domain.PostOffice;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;
using GreenEnergyHub.PostOffice.Communicator.DataAvailable;
using GreenEnergyHub.PostOffice.Communicator.Dequeue;
using GreenEnergyHub.PostOffice.Communicator.Model;
using GreenEnergyHub.PostOffice.Communicator.Peek;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice
{
    public class SubPostOfficeClient : ISubPostOfficeClient
    {
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IDataAvailableNotificationSender _dataAvailableNotificationSender;
        private readonly IDataBundleResponseSender _dataBundleResponseSender;
        private readonly IDequeueNotificationParser _dequeueNotificationParser;
        private readonly IRequestBundleParser _requestBundleParser;
        private readonly ISubPostOfficeStorageClient _subPostOfficeStorageClient;
        private readonly IPostOfficeStorageClient _postOfficeStorageClient;
        private readonly IPostOfficeMessageMetadataRepository _postOfficeMessageMetadataRepository;

        public SubPostOfficeClient(
            ISubPostOfficeStorageClient subPostOfficeStorageClient,
            IPostOfficeStorageClient postOfficeStorageClient,
            IPostOfficeMessageMetadataRepository postOfficeMessageMetadataRepository,
            IMessageDispatcher messageDispatcher,
            IDataAvailableNotificationSender dataAvailableNotificationSender,
            IDataBundleResponseSender dataBundleResponseSender,
            IDequeueNotificationParser dequeueNotificationParser,
            IRequestBundleParser requestBundleParser)
        {
            _subPostOfficeStorageClient = subPostOfficeStorageClient;
            _postOfficeStorageClient = postOfficeStorageClient;
            _postOfficeMessageMetadataRepository = postOfficeMessageMetadataRepository;
            _messageDispatcher = messageDispatcher;
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
            _dataBundleResponseSender = dataBundleResponseSender;
            _dequeueNotificationParser = dequeueNotificationParser;
            _requestBundleParser = requestBundleParser;
        }

        public async Task DispatchAsync(PostOfficeMessageEnvelope message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await _subPostOfficeStorageClient.WriteAsync(message).ConfigureAwait(false); // todo .. wrap message in messageblob with information about blobname etc.

            var messageMetadata = new PostOfficeMessageMetadata(message.Correlation);
            await _postOfficeMessageMetadataRepository.SaveMessageAsync(messageMetadata).ConfigureAwait(false);

            //TODO: Change DomainOrigin to MeteringPoints when added to enum.
            await _dataAvailableNotificationSender.SendAsync(new DataAvailableNotificationDto(messageMetadata.Id, new GlobalLocationNumberDto(message.Recipient), new MessageTypeDto(message.MessageType), DomainOrigin.MeteringPoints, true, 1)).ConfigureAwait(false);
        }

        public async Task CreateBundleAsync(byte[] request)
        {
            var notificationDto = _requestBundleParser.Parse(request);

            var messageMetadatas = await _postOfficeMessageMetadataRepository.GetMessagesAsync(notificationDto.DataAvailableNotificationIds.ToArray()).ConfigureAwait(false);

            StringBuilder fullMessage = new();

            foreach (var messageMetadata in messageMetadatas)
            {
                var message = await _subPostOfficeStorageClient.ReadAsync(messageMetadata.Id.ToString()).ConfigureAwait(false);
                fullMessage.Append(message);
            }

            await _postOfficeStorageClient.WriteAsync(Encoding.Default.GetBytes(fullMessage.ToString())).ConfigureAwait(false);

            await _dataBundleResponseSender.SendAsync(new RequestDataBundleResponseDto(new Uri("http://uriToBundleBlob"), notificationDto.DataAvailableNotificationIds), "sessionId").ConfigureAwait(false);
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
