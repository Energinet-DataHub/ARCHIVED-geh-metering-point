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
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;
using Energinet.DataHub.MeteringPoints.Messaging.Bundling.Xml;

namespace Energinet.DataHub.MeteringPoints.Messaging.Bundling.AccountingPointCharacteristics
{
    public class AccountingPointCharacteristicsMessageXmlSerializer : IDocumentSerializer<AccountingPointCharacteristicsMessage>
    {
        public AccountingPointCharacteristicsMessageXmlSerializer()
        {
            XmlDeclaration = new AccountingPointCharacteristicsXmlDeclaration();
            DocumentName = "AccountingPointCharacteristics_MarketDocument";
        }

        private XmlDeclaration XmlDeclaration { get; }

        private string DocumentName { get; }

        public string Serialize(AccountingPointCharacteristicsMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var document = CreateDocumentWithHeader(message);
            document
                ?.Element(XmlDeclaration.XmlNamespace + DocumentName)
                ?.Add(CreateMarketActivityRecord(message));

            return Serialize(document!);
        }

        public string Serialize(IList<AccountingPointCharacteristicsMessage> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            var document = CreateDocumentWithHeader(messages.First());
            foreach (var message in messages)
            {
                document
                    ?.Element(XmlDeclaration.XmlNamespace + DocumentName)
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

        private static List<XElement> MridElement(XNamespace xmlNamespace, string name, Mrid? mrid)
        {
            return mrid == null
                ? new List<XElement>() // Optional for some
                : new List<XElement> { new(xmlNamespace + name, new XAttribute("codingScheme", mrid.CodingScheme), mrid.Id) };
        }

        private static XElement UnitElement(XNamespace xmlNamespace, string name, UnitValue unitValue)
        {
            return new XElement(xmlNamespace + name, new XAttribute("unit", unitValue.Unit), unitValue.Value);
        }

        private static XElement MarketParticipantElement(XNamespace xmlNamespace, string name, MarketParticipant marketParticipant)
        {
            return new XElement(xmlNamespace + name, new XAttribute("codingScheme", marketParticipant.CodingScheme), marketParticipant.Id);
        }

        private XElement CreateMarketActivityRecord(AccountingPointCharacteristicsMessage message)
        {
            return new XElement(
                XmlDeclaration.XmlNamespace + "MktActivityRecord",
                new XElement(XmlDeclaration.XmlNamespace + "mRID", message.MarketActivityRecord.Id),
                new XElement(XmlDeclaration.XmlNamespace + "originalTransactionIDReference_MktActivityRecord.mRID", message.MarketActivityRecord.OriginalTransaction),
                new XElement(XmlDeclaration.XmlNamespace + "validityStart_DateAndOrTime.dateTime", message.MarketActivityRecord.ValidityStartDateAndOrTime),
                new XElement(
                XmlDeclaration.XmlNamespace + "MarketEvaluationPoint",
                new XElement(XmlDeclaration.XmlNamespace + "mRID", new XAttribute("codingScheme", message.MarketActivityRecord.MarketEvaluationPoint.Id.CodingScheme), message.MarketActivityRecord.MarketEvaluationPoint.Id.Id),
                MarketParticipantElement(XmlDeclaration.XmlNamespace, "meteringPointResponsible_MarketParticipant.mRID", message.MarketActivityRecord.MarketEvaluationPoint.MeteringPointResponsibleMarketRoleParticipant),
                new XElement(XmlDeclaration.XmlNamespace + "type", message.MarketActivityRecord.MarketEvaluationPoint.Type),
                new XElement(XmlDeclaration.XmlNamespace + "settlementMethod", message.MarketActivityRecord.MarketEvaluationPoint.SettlementMethod),
                new XElement(XmlDeclaration.XmlNamespace + "meteringMethod", message.MarketActivityRecord.MarketEvaluationPoint.MeteringMethod),
                new XElement(XmlDeclaration.XmlNamespace + "connectionState", message.MarketActivityRecord.MarketEvaluationPoint.ConnectionState),
                new XElement(XmlDeclaration.XmlNamespace + "readCycle", message.MarketActivityRecord.MarketEvaluationPoint.ReadCycle),
                new XElement(XmlDeclaration.XmlNamespace + "netSettlementGroup", message.MarketActivityRecord.MarketEvaluationPoint.NetSettlementGroup),
                new XElement(XmlDeclaration.XmlNamespace + "nextReadingDate", message.MarketActivityRecord.MarketEvaluationPoint.NextReadingDate),
                MridElement(XmlDeclaration.XmlNamespace, "meteringGridArea_Domain.mRID", message.MarketActivityRecord.MarketEvaluationPoint.MeteringGridAreaDomainId),
                MridElement(XmlDeclaration.XmlNamespace, "inMeteringGridArea_Domain.mRID", message.MarketActivityRecord.MarketEvaluationPoint.InMeteringGridAreaDomainId),
                MridElement(XmlDeclaration.XmlNamespace, "outMeteringGridArea_Domain.mRID", message.MarketActivityRecord.MarketEvaluationPoint.OutMeteringGridAreaDomainId),
                MridElement(XmlDeclaration.XmlNamespace, "linked_MarketEvaluationPoint.mRID", message.MarketActivityRecord.MarketEvaluationPoint.LinkedMarketEvaluationPoint),
                UnitElement(XmlDeclaration.XmlNamespace, "physicalConnectionCapacity", message.MarketActivityRecord.MarketEvaluationPoint.PhysicalConnectionCapacity),
                new XElement(XmlDeclaration.XmlNamespace + "mPConnectionType", message.MarketActivityRecord.MarketEvaluationPoint.ConnectionType),
                new XElement(XmlDeclaration.XmlNamespace + "disconnectionMethod", message.MarketActivityRecord.MarketEvaluationPoint.DisconnectionMethod),
                new XElement(XmlDeclaration.XmlNamespace + "asset_MktPSRType.psrType", message.MarketActivityRecord.MarketEvaluationPoint.AssetMarketPSRTypePsrType),
                new XElement(XmlDeclaration.XmlNamespace + "productionObligation", message.MarketActivityRecord.MarketEvaluationPoint.ProductionObligation),
                UnitElement(XmlDeclaration.XmlNamespace, "contractedConnectionCapacity", message.MarketActivityRecord.MarketEvaluationPoint.ContractedConnectionCapacity),
                UnitElement(XmlDeclaration.XmlNamespace, "ratedCurrent", message.MarketActivityRecord.MarketEvaluationPoint.RatedCurrent),
                new XElement(XmlDeclaration.XmlNamespace + "meter.mRID", message.MarketActivityRecord.MarketEvaluationPoint.MeterId),
                new XElement(
                    XmlDeclaration.XmlNamespace + "Series",
                    new XElement(XmlDeclaration.XmlNamespace + "product", message.MarketActivityRecord.MarketEvaluationPoint.Series.Product),
                    new XElement(XmlDeclaration.XmlNamespace + "quantity_Measure_Unit.name", message.MarketActivityRecord.MarketEvaluationPoint.Series.QuantityMeasureUnit)),
                MarketParticipantElement(XmlDeclaration.XmlNamespace, "energySupplier_MarketParticipant.mRID", message.MarketActivityRecord.MarketEvaluationPoint.EnergySupplierMarketParticipantId),
                new XElement(XmlDeclaration.XmlNamespace + "supplyStart_DateAndOrTime.dateTime", message.MarketActivityRecord.MarketEvaluationPoint.SupplyStartDateAndOrTimeDateTime),
                new XElement(XmlDeclaration.XmlNamespace + "description", message.MarketActivityRecord.MarketEvaluationPoint.Description),
                new XElement(XmlDeclaration.XmlNamespace + "usagePointLocation.geoInfoReference", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationGeoInfoReference),
                new XElement(
                    XmlDeclaration.XmlNamespace + "usagePointLocation.mainAddress",
                    new XElement(
                        XmlDeclaration.XmlNamespace + "streetDetail",
                        new XElement(XmlDeclaration.XmlNamespace + "code", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.Code),
                        new XElement(XmlDeclaration.XmlNamespace + "name", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.Name),
                        new XElement(XmlDeclaration.XmlNamespace + "number", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.Number),
                        new XElement(XmlDeclaration.XmlNamespace + "floorIdentification", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.FloorIdentification),
                        new XElement(XmlDeclaration.XmlNamespace + "suiteNumber", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.SuiteNumber)),
                    new XElement(
                        XmlDeclaration.XmlNamespace + "townDetail",
                        new XElement(XmlDeclaration.XmlNamespace + "code", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Code),
                        new XElement(XmlDeclaration.XmlNamespace + "name", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Name),
                        new XElement(XmlDeclaration.XmlNamespace + "section", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Section),
                        new XElement(XmlDeclaration.XmlNamespace + "country", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Country)),
                    new XElement(XmlDeclaration.XmlNamespace + "postalCode", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.PostalCode)),
                new XElement(XmlDeclaration.XmlNamespace + "usagePointLocation.actualAddressIndicator", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationActualAddressIndicator),
                new XElement(
                    XmlDeclaration.XmlNamespace + "Parent_MarketEvaluationPoint",
                    new XElement(XmlDeclaration.XmlNamespace + "mRID", message.MarketActivityRecord.MarketEvaluationPoint.ParentMarketEvaluationPoint.Id, new XAttribute("codingScheme", message.MarketActivityRecord.MarketEvaluationPoint.ParentMarketEvaluationPoint.CodingScheme)),
                    new XElement(XmlDeclaration.XmlNamespace + "description", message.MarketActivityRecord.MarketEvaluationPoint.ParentMarketEvaluationPoint.Description)),
                new XElement(
                    XmlDeclaration.XmlNamespace + "Child_MarketEvaluationPoint",
                    new XElement(XmlDeclaration.XmlNamespace + "mRID", message.MarketActivityRecord.MarketEvaluationPoint.ChildMarketEvaluationPoint.Id, new XAttribute("codingScheme", message.MarketActivityRecord.MarketEvaluationPoint.ChildMarketEvaluationPoint.CodingScheme)),
                    new XElement(XmlDeclaration.XmlNamespace + "description", message.MarketActivityRecord.MarketEvaluationPoint.ChildMarketEvaluationPoint.Description))));
        }

        private XDocument CreateDocumentWithHeader(AccountingPointCharacteristicsMessage message)
        {
            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            XNamespace schemaLocation = XNamespace.Get($"{XmlDeclaration.XmlNamespace} {XmlDeclaration.SchemaLocationText}");

            var document = new XDocument(
                new XElement(
                    XmlDeclaration.XmlNamespace + DocumentName,
                    new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                    new XAttribute(XNamespace.Xmlns + "cim", XmlDeclaration.XmlNamespace),
                    new XAttribute(xsi + "schemaLocation", schemaLocation),
                    new XElement(XmlDeclaration.XmlNamespace + "mRID", Guid.NewGuid().ToString()),
                    new XElement(XmlDeclaration.XmlNamespace + "type", message.Type),
                    new XElement(XmlDeclaration.XmlNamespace + "process.processType", message.ProcessType),
                    new XElement(XmlDeclaration.XmlNamespace + "businessSector.type", message.BusinessSectorType),
                    new XElement(XmlDeclaration.XmlNamespace + "sender_MarketParticipant.mRID", new XAttribute("codingScheme", message.Sender.CodingScheme), message.Sender.Id),
                    new XElement(XmlDeclaration.XmlNamespace + "sender_MarketParticipant.marketRole.type", message.Sender.Role),
                    new XElement(XmlDeclaration.XmlNamespace + "receiver_MarketParticipant.mRID", new XAttribute("codingScheme", message.Receiver.CodingScheme), message.Receiver.Id),
                    new XElement(XmlDeclaration.XmlNamespace + "receiver_MarketParticipant.marketRole.type", message.Receiver.Role),
                    new XElement(XmlDeclaration.XmlNamespace + "createdDateTime", message.CreatedDateTime)));

            return document;
        }
    }
}
