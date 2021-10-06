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
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Acknowledgements;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.SubPostOffice.Bundling
{
    public class ConfirmMessageSerializer : IDocumentSerializer<ConfirmMessage>
    {
        public string Serialize(ConfirmMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            XNamespace xmlNamespace = "urn:ebix:org:ChangeAccountingPointCharacteristics:0:1";

            var document = CreateDocumentWithHeader(message, xmlNamespace);
            document
                ?.Element(xmlNamespace + message.DocumentName)
                ?.Add(CreateMarketActivityRecord(message, xmlNamespace));

            return Serialize(document!);
        }

        public string Serialize(IList<ConfirmMessage> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            XNamespace xmlNamespace = "urn:ebix:org:ChangeAccountingPointCharacteristics:0:1";

            var document = CreateDocumentWithHeader(messages.First(), xmlNamespace);
            foreach (var message in messages)
            {
                document
                    ?.Element(xmlNamespace + message.DocumentName)
                    ?.Add(CreateMarketActivityRecord(message, xmlNamespace));
            }

            return Serialize(document!);
        }

        private static XElement CreateMarketActivityRecord(ConfirmMessage message, XNamespace xmlNamespace)
        {
            return new XElement(
                xmlNamespace + "MktActivityRecord",
                new XElement(xmlNamespace + "mRID", message.MarketActivityRecord.Id),
                new XElement(xmlNamespace + "businessProcessReference_MktActivityRecord.mRID", message.MarketActivityRecord.BusinessProcessReference),
                new XElement(xmlNamespace + "originalTransactionReference_MktActivityRecord.mRID", message.MarketActivityRecord.OriginalTransaction),
                new XElement(xmlNamespace + "marketEvaluationPoint.mRID", message.MarketActivityRecord.MarketEvaluationPoint),
                new XElement(xmlNamespace + "start_DateAndOrTime.date", message.MarketActivityRecord.StartDateAndOrTime));
        }

        private static XDocument CreateDocumentWithHeader(ConfirmMessage message, XNamespace xmlNamespace)
        {
            var document = new XDocument(
                new XElement(
                    xmlNamespace + message.DocumentName,
                    new XAttribute(XNamespace.Xmlns + "cim", xmlNamespace),
                    new XElement(xmlNamespace + "mRID", Guid.NewGuid().ToString()),
                    new XElement(xmlNamespace + "type", message.Type),
                    new XElement(xmlNamespace + "process.processType", message.ProcessType),
                    new XElement(xmlNamespace + "businessSector.type", message.BusinessSectorType),
                    new XElement(xmlNamespace + "sender_MarketParticipant.mRID", new XAttribute("codingScheme", message.Sender.CodingScheme), message.Sender.Id),
                    new XElement(xmlNamespace + "sender_MarketParticipant.marketRole.type", message.Sender.Role),
                    new XElement(xmlNamespace + "receiver_MarketParticipant.mRID", new XAttribute("codingScheme", message.Receiver.CodingScheme), message.Receiver.Id),
                    new XElement(xmlNamespace + "receiver_MarketParticipant.marketRole.type", message.Receiver.Role),
                    new XElement(xmlNamespace + "createdDateTime", message.CreatedDateTime),
                    new XElement(xmlNamespace + "reason.code", message.ReasonCode)));

            return document;
        }

        private static string Serialize(XDocument document)
        {
            using var writer = new Utf8StringWriter();
            document.Save(writer);
            return writer.ToString();
        }
    }
}
