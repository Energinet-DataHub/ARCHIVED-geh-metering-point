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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MessageHub.Bundling;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;

namespace Energinet.DataHub.MeteringPoints.Messaging.Bundling
{
    public class ConfirmMessageSerializer : IDocumentSerializer<ConfirmMessage>
    {
        private readonly XNamespace _xmlNamespace = "urn:ediel.org:structure:confirmrequestchangeofaccountingpointcharacteristics:0:1";

        public string Serialize(ConfirmMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var document = CreateDocumentWithHeader(message);
            document
                ?.Element(_xmlNamespace + message.DocumentName)
                ?.Add(CreateMarketActivityRecord(message));

            return Serialize(document!);
        }

        public string Serialize(IList<ConfirmMessage> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            var document = CreateDocumentWithHeader(messages.First());
            foreach (var message in messages)
            {
                document
                    ?.Element(_xmlNamespace + message.DocumentName)
                    ?.Add(CreateMarketActivityRecord(message));
            }

            return Serialize(document!);
        }

        private static string Serialize(XDocument document)
        {
            using var writer = new Utf8StringWriter();
            document.Save(writer);
            return writer.ToString();
        }

        private XElement CreateMarketActivityRecord(ConfirmMessage message)
        {
            return new XElement(
                _xmlNamespace + "MktActivityRecord",
                new XElement(_xmlNamespace + "mRID", message.MarketActivityRecord.Id),
                new XElement(_xmlNamespace + "originalTransactionReference_MktActivityRecord.mRID", message.MarketActivityRecord.OriginalTransaction),
                new XElement(_xmlNamespace + "marketEvaluationPoint.mRID", message.MarketActivityRecord.MarketEvaluationPoint));
        }

        private XDocument CreateDocumentWithHeader(ConfirmMessage message)
        {
            var document = new XDocument(
                new XElement(
                    _xmlNamespace + message.DocumentName,
                    new XAttribute(XNamespace.Xmlns + "cim", _xmlNamespace),
                    new XElement(_xmlNamespace + "mRID", Guid.NewGuid().ToString()),
                    new XElement(_xmlNamespace + "type", message.Type),
                    new XElement(_xmlNamespace + "process.processType", message.ProcessType),
                    new XElement(_xmlNamespace + "businessSector.type", message.BusinessSectorType),
                    new XElement(_xmlNamespace + "sender_MarketParticipant.mRID", new XAttribute("codingScheme", message.Sender.CodingScheme), message.Sender.Id),
                    new XElement(_xmlNamespace + "sender_MarketParticipant.marketRole.type", message.Sender.Role),
                    new XElement(_xmlNamespace + "receiver_MarketParticipant.mRID", new XAttribute("codingScheme", message.Receiver.CodingScheme), message.Receiver.Id),
                    new XElement(_xmlNamespace + "receiver_MarketParticipant.marketRole.type", message.Receiver.Role),
                    new XElement(_xmlNamespace + "createdDateTime", message.CreatedDateTime),
                    new XElement(_xmlNamespace + "reason.code", message.ReasonCode)));

            return document;
        }
    }
}
