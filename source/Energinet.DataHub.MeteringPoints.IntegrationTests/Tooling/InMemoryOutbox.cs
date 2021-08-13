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

using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling
{
    public class InMemoryOutbox : IOutbox, IOutboxManager
    {
        private readonly List<OutboxMessage> _messages = new();

        public void Add(OutboxMessage message)
        {
            _messages.Add(message);
        }

#pragma warning disable 8632 // Nullable not enabled in test project
        public OutboxMessage? GetNext()
        {
            return _messages.FirstOrDefault();
        }

        public OutboxMessage? GetNext(OutboxMessageCategory category)
#pragma warning restore 8632
        {
            return _messages.FirstOrDefault(message => message.Category == category);
        }

#pragma warning disable 8632 // Nullable not enabled in test project
        public OutboxMessage? GetNext(OutboxMessageCategory category, string type)
        {
            return _messages.FirstOrDefault(message => message.Category == category && message.Type == type);
        }
#pragma warning restore 8632

        public void MarkProcessed(OutboxMessage outboxMessage)
        {
            _messages.Remove(outboxMessage);
        }
    }
}
