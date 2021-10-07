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
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice
{
    public class PostOfficeMessage
    {
        public PostOfficeMessage(string content, string correlation, DocumentType type, string recipient, Instant date)
        {
            Id = Guid.NewGuid();
            Correlation = correlation;
            Type = type;
            Recipient = recipient;
            Date = date;
            Content = content;
        }

        public PostOfficeMessage(Guid id, string messageContent, string correlation, DocumentType type, string recipient, Instant date)
            : this(messageContent, correlation, type, recipient, date)
        {
            Id = id;
        }

        public Guid Id { get; }

        public string Content { get; }

        public string Correlation { get; }

        public DocumentType Type { get; }

        public string Recipient { get; }

        public Instant Date { get; }
    }
}
