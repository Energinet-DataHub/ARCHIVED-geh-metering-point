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
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using GreenEnergyHub.PostOffice.Communicator.DataAvailable;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice
{
    public class SubPostOfficeDataAvailableClient : ISubPostOfficeDataAvailableClient
    {
        private readonly IDataAvailableNotificationSender _dataAvailableNotificationSender;
        private readonly PostOfficeMessageFactory _postOfficeMessageFactory;
        private readonly IPostOfficeMessageMetadataRepository _postOfficeMessageMetadataRepository;

        public SubPostOfficeDataAvailableClient(
            IPostOfficeMessageMetadataRepository postOfficeMessageMetadataRepository,
            IDataAvailableNotificationSender dataAvailableNotificationSender,
            PostOfficeMessageFactory postOfficeMessageFactory)
        {
            _postOfficeMessageMetadataRepository = postOfficeMessageMetadataRepository;
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
            _postOfficeMessageFactory = postOfficeMessageFactory;
        }

        public async Task DataAvailableAsync(PostOfficeMessageEnvelope message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var messageMetadata = _postOfficeMessageFactory.Create(message.Correlation, message.Content, message.MessageType, message.Recipient);
            _postOfficeMessageMetadataRepository.AddMessageMetadata(messageMetadata);

            // TODO - add notification to Outbox instead of sending immediately
            await _dataAvailableNotificationSender.SendAsync(new DataAvailableNotificationDto(messageMetadata.Id, new GlobalLocationNumberDto(message.Recipient), new MessageTypeDto(message.MessageType.Name), DomainOrigin.MeteringPoints, true, 1)).ConfigureAwait(false);
        }
    }
}
