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
        private readonly IPostOfficeMessageMetadataRepository _postOfficeMessageMetadataRepository;

        public SubPostOfficeDataAvailableClient(
            IPostOfficeMessageMetadataRepository postOfficeMessageMetadataRepository,
            IDataAvailableNotificationSender dataAvailableNotificationSender)
        {
            _postOfficeMessageMetadataRepository = postOfficeMessageMetadataRepository;
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
        }

        public async Task DataAvailableAsync(PostOfficeMessageEnvelope message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var messageMetadata = PostOfficeMessageFactory.Create(message.Correlation, message.Content);
            await _postOfficeMessageMetadataRepository.SaveMessageMetadataAsync(messageMetadata).ConfigureAwait(false);

            await _dataAvailableNotificationSender.SendAsync(new DataAvailableNotificationDto(messageMetadata.Id, new GlobalLocationNumberDto(message.Recipient), new MessageTypeDto(message.MessageType), DomainOrigin.MeteringPoints, true, 1)).ConfigureAwait(false);
        }
    }
}
