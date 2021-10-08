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
using Energinet.DataHub.MeteringPoints.Application.Integrations;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Application.ChargeLinks
{
    public class ChargeLinksRequestMetadata : IIntegrationMetadataContext
    {
        public ChargeLinksRequestMetadata(string correlationId, string replyToPath, string processId, string requestId)
        {
            CorrelationId = correlationId;
            ReplyToPath = replyToPath;
            ProcessId = processId;
            RequestId = requestId;
        }

        public string RequestId { get; }

        public string ProcessId { get; }

        public string ReplyToPath { get; }

        public Instant Timestamp { get; private set; }

        public string CorrelationId { get; private set; }

        public Guid EventId { get; private set; }

        public void SetMetadata(Instant timestamp, string correlationId, Guid eventId)
        {
            Timestamp = timestamp;
            CorrelationId = correlationId;
            EventId = eventId;
        }
    }
}
