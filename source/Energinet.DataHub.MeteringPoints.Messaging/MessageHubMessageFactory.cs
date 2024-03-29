﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using Energinet.DataHub.MeteringPoints.Infrastructure.LocalMessageHub;

namespace Energinet.DataHub.MeteringPoints.Messaging
{
    public class MessageHubMessageFactory
    {
        private readonly ISystemDateTimeProvider _dateTimeProvider;

        public MessageHubMessageFactory(ISystemDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public MessageHubMessage Create(
            string correlation,
            string messageContent,
            DocumentType type,
            string recipient,
            string gsrnNumber)
        {
            return new MessageHubMessage(messageContent, correlation, type, recipient, _dateTimeProvider.Now(), gsrnNumber);
        }
    }
}
