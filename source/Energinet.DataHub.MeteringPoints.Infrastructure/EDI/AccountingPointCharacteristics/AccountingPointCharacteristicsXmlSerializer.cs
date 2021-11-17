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
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics
{
    public static class AccountingPointCharacteristicsXmlSerializer
    {
        public static string Serialize(AccountingPointCharacteristicsMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            XNamespace xmlns = XNamespace.Get("urn:ediel.org:structure:accountingpointcharacteristics:0:1");
            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            XNamespace schemaLocation = XNamespace.Get("urn:ediel.org:structure:accountingpointcharacteristics:0:1 urn-ediel-org-structure-accountingpointcharacteristics-0-1.xsd");

            var document = new XDocument(
                new XElement(
                    xmlns + "AccountingPointcharacteristics_MarketDocument",
                    new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                    new XAttribute(XNamespace.Xmlns + "cim", xmlns),
                    new XAttribute(xsi + "schemaLocation", schemaLocation),
                    new XElement(xmlns + "mRID", message.Id),
                    new XElement(xmlns + "type", message.Type),
                    new XElement(xmlns + "process.processType", message.ProcessType),
                    new XElement(xmlns + "businessSector.type", message.BusinessSectorType),
                    new XElement(xmlns + "sender_MarketParticipant.mRID", new XAttribute("codingScheme", message.Sender.CodingScheme), message.Sender.Id),
                    new XElement(xmlns + "sender_MarketParticipant.marketRole.type", message.Sender.Role),
                    new XElement(xmlns + "receiver_MarketParticipant.mRID", new XAttribute("codingScheme", message.Receiver.CodingScheme), message.Receiver.Id),
                    new XElement(xmlns + "receiver_MarketParticipant.marketRole.type", message.Receiver.Role),
                    new XElement(xmlns + "createdDateTime", message.CreatedDateTime),
                    new XElement(
                        xmlns + "MktActivityRecord",
                        new XElement(xmlns + "mRID", message.MarketActivityRecord.Id),
                        new XElement(xmlns + "originalTransactionIDReference_MktActivityRecord.mRID", message.MarketActivityRecord.OriginalTransaction),
                        new XElement(xmlns + "validityStart_DateAndOrTime.dateTime", message.MarketActivityRecord.ValidityStartDateAndOrTime),
                        new XElement(
                            xmlns + "MarketEvaluationPoint",
                            new XElement(xmlns + "mRID", new XAttribute("codingScheme", message.MarketActivityRecord.MarketEvaluationPoint.Id.CodingScheme), message.MarketActivityRecord.MarketEvaluationPoint.Id.Id),
                            MarketParticipantElement(xmlns, "meteringPointResponsible_MarketParticipant.mRID", message.MarketActivityRecord.MarketEvaluationPoint.MeteringPointResponsibleMarketRoleParticipant),
                            new XElement(xmlns + "type", message.MarketActivityRecord.MarketEvaluationPoint.Type),
                            new XElement(xmlns + "settlementMethod", message.MarketActivityRecord.MarketEvaluationPoint.SettlementMethod),
                            new XElement(xmlns + "meteringMethod", message.MarketActivityRecord.MarketEvaluationPoint.MeteringMethod),
                            new XElement(xmlns + "connectionState", message.MarketActivityRecord.MarketEvaluationPoint.ConnectionState),
                            new XElement(xmlns + "readCycle", message.MarketActivityRecord.MarketEvaluationPoint.ReadCycle),
                            new XElement(xmlns + "netSettlementGroup", message.MarketActivityRecord.MarketEvaluationPoint.NetSettlementGroup),
                            new XElement(xmlns + "nextReadingDate", message.MarketActivityRecord.MarketEvaluationPoint.NextReadingDate),
                            MridElement(xmlns, "meteringGridArea_Domain.mRID", message.MarketActivityRecord.MarketEvaluationPoint.MeteringGridAreaDomainId),
                            MridElement(xmlns, "inMeteringGridArea_Domain.mRID", message.MarketActivityRecord.MarketEvaluationPoint.InMeteringGridAreaDomainId),
                            MridElement(xmlns, "outMeteringGridArea_Domain.mRID", message.MarketActivityRecord.MarketEvaluationPoint.OutMeteringGridAreaDomainId),
                            new XElement(xmlns + "linked_MarketEvaluationPoint.mRID", message.MarketActivityRecord.MarketEvaluationPoint.LinkedMarketEvaluationPoint),
                            UnitElement(xmlns, "physicalConnectionCapacity", message.MarketActivityRecord.MarketEvaluationPoint.PhysicalConnectionCapacity),
                            new XElement(xmlns + "mPConnectionType", message.MarketActivityRecord.MarketEvaluationPoint.ConnectionType),
                            new XElement(xmlns + "disconnectionMethod", message.MarketActivityRecord.MarketEvaluationPoint.DisconnectionMethod),
                            new XElement(xmlns + "asset_MktPSRType.psrType", message.MarketActivityRecord.MarketEvaluationPoint.AssetMarketPSRTypePsrType),
                            new XElement(xmlns + "productionObligation", message.MarketActivityRecord.MarketEvaluationPoint.ProductionObligation),
                            UnitElement(xmlns, "contractedConnectionCapacity", message.MarketActivityRecord.MarketEvaluationPoint.ContractedConnectionCapacity),
                            UnitElement(xmlns, "ratedCurrent", message.MarketActivityRecord.MarketEvaluationPoint.RatedCurrent),
                            new XElement(xmlns + "meter.mRID", message.MarketActivityRecord.MarketEvaluationPoint.MeterId),
                            MarketParticipantElement(xmlns, "energySupplier_MarketParticipant.mRID", message.MarketActivityRecord.MarketEvaluationPoint.EnergySupplierMarketParticipantId),
                            new XElement(xmlns + "supplyStart_DateAndOrTime.dateTime", message.MarketActivityRecord.MarketEvaluationPoint.SupplyStartDateAndOrTimeDateTime),
                            new XElement(
                                xmlns + "Series",
                                new XElement(xmlns + "product", message.MarketActivityRecord.MarketEvaluationPoint.Series.Product),
                                new XElement(xmlns + "quantity_Measure_Unit.name", message.MarketActivityRecord.MarketEvaluationPoint.Series.QuantityMeasureUnit)),
                            new XElement(xmlns + "description", message.MarketActivityRecord.MarketEvaluationPoint.Description),
                            new XElement(xmlns + "usagePointLocation.geoInfoReference", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationGeoInfoReference),
                            new XElement(
                                xmlns + "usagePointLocation.mainAddress",
                                new XElement(
                                    xmlns + "streetDetail",
                                    new XElement(xmlns + "code", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.Code),
                                    new XElement(xmlns + "name", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.Name),
                                    new XElement(xmlns + "number", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.Number),
                                    new XElement(xmlns + "floorIdentification", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.FloorIdentification),
                                    new XElement(xmlns + "suiteNumber", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.SuiteNumber)),
                                new XElement(
                                    xmlns + "townDetail",
                                    new XElement(xmlns + "code", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Code),
                                    new XElement(xmlns + "name", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Name),
                                    new XElement(xmlns + "section", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Section),
                                    new XElement(xmlns + "country", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Country)),
                                new XElement(xmlns + "postalCode", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.PostalCode)),
                            new XElement(xmlns + "usagePointLocation.actualAddressIndicator", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationActualAddressIndicator),
                            new XElement(
                                xmlns + "Parent_MarketEvaluationPoint",
                                new XElement(xmlns + "mRID", message.MarketActivityRecord.MarketEvaluationPoint.ParentMarketEvaluationPoint.Id)),
                            new XElement(
                                xmlns + "Child_MarketEvaluationPoint",
                                new XElement(xmlns + "mRID", message.MarketActivityRecord.MarketEvaluationPoint.ChildMarketEvaluationPoint.Id),
                                new XElement(xmlns + "description", message.MarketActivityRecord.MarketEvaluationPoint.ChildMarketEvaluationPoint.Description))))));
            return Serialize(document);
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

        private static string Serialize(XDocument document)
        {
            using var writer = new Utf8StringWriter();
            document.Save(writer);
            return writer.ToString();
        }
    }
}
