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

namespace Energinet.DataHub.MeteringPoints.Infrastructure.LocalMessageHub.Bundling
{
    public class RejectMessageSerializer : IDocumentSerializer<RejectMessage>
    {
        private readonly XNamespace _xmlNamespace = "urn:ediel.org:structure:rejectrequestchangeofaccountingpointcharacteristics:0:1";

        public string Serialize(RejectMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var document = CreateDocumentWithHeader(message);
            document
                ?.Element(_xmlNamespace + message.DocumentName)
                ?.Add(CreateMarketActivityRecord(message));

            return Serialize(document!);
        }

        public string Serialize(IList<RejectMessage> messages)
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

        private XElement CreateMarketActivityRecord(RejectMessage message)
        {
            return new XElement(
                _xmlNamespace + "MktActivityRecord",
                new XElement(_xmlNamespace + "mRID", message.MarketActivityRecord.Id),
                new XElement(_xmlNamespace + "businessProcessReference_MktActivityRecord.mRID", message.MarketActivityRecord.BusinessProcessReference),
                new XElement(_xmlNamespace + "originalTransactionReference_MktActivityRecord.mRID", message.MarketActivityRecord.OriginalTransaction),
                new XElement(_xmlNamespace + "marketEvaluationPoint.mRID", message.MarketActivityRecord.MarketEvaluationPoint),
                new XElement(_xmlNamespace + "start_DateAndOrTime.date", message.MarketActivityRecord.StartDateAndOrTime),
                message.MarketActivityRecord.Reasons.Select(r => GetReasonElement(r.Code, r.Text)));
        }

        private XDocument CreateDocumentWithHeader(RejectMessage message)
        {
            var document = new XDocument(
                new XElement(
                    _xmlNamespace + message.DocumentName,
                    new XAttribute(XNamespace.Xmlns + "cim", _xmlNamespace),
                    new XElement(_xmlNamespace + "mRID", message.Id),
                    new XElement(_xmlNamespace + "type", message.Type),
                    new XElement(_xmlNamespace + "process.processType", message.ProcessType),
                    new XElement(_xmlNamespace + "businessSector.type", message.BusinessSectorType),
                    new XElement(_xmlNamespace + "sender_MarketParticipant.mRID", new XAttribute("codingScheme", message.Sender.CodingScheme), message.Sender.Id),
                    new XElement(_xmlNamespace + "sender_MarketParticipant.marketRole.type", message.Sender.Role),
                    new XElement(_xmlNamespace + "receiver_MarketParticipant.mRID", new XAttribute("codingScheme", message.Receiver.CodingScheme), message.Receiver.Id),
                    new XElement(_xmlNamespace + "receiver_MarketParticipant.marketRole.type", message.Receiver.Role),
                    new XElement(_xmlNamespace + "createdDateTime", message.CreatedDateTime),
                    GetReasonElement(message.Reason.Code, message.Reason.Text)));

            return document;
        }

        private XElement GetReasonElement(string code, string text)
        {
            return new XElement(
                _xmlNamespace + "Reason",
                new XElement(_xmlNamespace + "code", code),
                new XElement(_xmlNamespace + "text", text));
        }
    }
}
