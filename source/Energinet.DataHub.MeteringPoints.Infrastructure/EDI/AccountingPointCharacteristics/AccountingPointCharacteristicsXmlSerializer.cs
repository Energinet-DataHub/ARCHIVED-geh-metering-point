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
using System.Xml.Linq;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.Common;
using Energinet.DataHub.MeteringPoints.Infrastructure.Serialization;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.AccountingPointCharacteristics
{
    public static class AccountingPointCharacteristicsXmlSerializer
    {
        public static string Serialize(AccountingPointCharacteristicsMessage message, XNamespace xmlNamespace)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var document = new XDocument(
                new XElement(
                    xmlNamespace + "AccountingPointcharacteristics_MarketDocument",
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
                    new XElement(
                        xmlNamespace + "MktActivityRecord",
                        new XElement(xmlNamespace + "mRID", message.MarketActivityRecord.Id),
                        new XElement(xmlNamespace + "businessProcessReference_MktActivityRecord.mRID", message.MarketActivityRecord.BusinessProcessReference),
                        new XElement(xmlNamespace + "originalTransactionReference_MktActivityRecord.mRID", message.MarketActivityRecord.OriginalTransaction),
                        new XElement(xmlNamespace + "start_DateAndOrTime.date", message.MarketActivityRecord.ValidityStartDateAndOrTime),
                        new XElement(
                            xmlNamespace + "MarketEvaluationPoint",
                            new XElement(xmlNamespace + "mRID", new XAttribute("codingScheme", message.MarketActivityRecord.MarketEvaluationPoint.Id.CodingScheme), message.MarketActivityRecord.MarketEvaluationPoint.Id.Id),
                            MarketParticipantElement(xmlNamespace, "meteringGridArea_Domain.mRID", message.MarketActivityRecord.MarketEvaluationPoint.MeteringPointResponsibleMarketRoleParticipant),
                            new XElement(xmlNamespace + "type", message.MarketActivityRecord.MarketEvaluationPoint.Type),
                            new XElement(xmlNamespace + "settlementMethod", message.MarketActivityRecord.MarketEvaluationPoint.SettlementMethod),
                            new XElement(xmlNamespace + "meteringMethod", message.MarketActivityRecord.MarketEvaluationPoint.MeteringMethod),
                            new XElement(xmlNamespace + "connectionState", message.MarketActivityRecord.MarketEvaluationPoint.ConnectionState),
                            new XElement(xmlNamespace + "readCycle", message.MarketActivityRecord.MarketEvaluationPoint.ReadCycle),
                            new XElement(xmlNamespace + "netSettlementGroup", message.MarketActivityRecord.MarketEvaluationPoint.NetSettlementGroup),
                            new XElement(xmlNamespace + "nextReadingDate", message.MarketActivityRecord.MarketEvaluationPoint.NextReadingDate),
                            MridElement(xmlNamespace, "meteringGridArea_Domain.mRID", message.MarketActivityRecord.MarketEvaluationPoint.MeteringGridAreaDomainId),
                            MridElement(xmlNamespace, "inMeteringGridArea_Domain.mRID", message.MarketActivityRecord.MarketEvaluationPoint.InMeteringGridAreaDomainId),
                            MridElement(xmlNamespace, "outMeteringGridArea_Domain.mRID", message.MarketActivityRecord.MarketEvaluationPoint.OutMeteringGridAreaDomainId),
                            MridElement(xmlNamespace, "Linked_MarketEvaluationPoint.mRID", message.MarketActivityRecord.MarketEvaluationPoint.LinkedMarketEvaluationPoint),
                            UnitElement(xmlNamespace, "physicalConnectionCapacity", message.MarketActivityRecord.MarketEvaluationPoint.PhysicalConnectionCapacity),
                            new XElement(xmlNamespace + "mPConnectionType", message.MarketActivityRecord.MarketEvaluationPoint.ConnectionType),
                            new XElement(xmlNamespace + "disconnectionMethod", message.MarketActivityRecord.MarketEvaluationPoint.DisconnectionMethod),
                            new XElement(xmlNamespace + "asset_MktPSRType.psrType", message.MarketActivityRecord.MarketEvaluationPoint.AssetMarketPSRTypePsrType),
                            new XElement(xmlNamespace + "productionObligation", message.MarketActivityRecord.MarketEvaluationPoint.ProductionObligation),
                            new XElement(
                                xmlNamespace + "Series",
                                new XElement(xmlNamespace + "mRID", message.MarketActivityRecord.MarketEvaluationPoint.Series.Id),
                                new XElement(xmlNamespace + "estimatedAnnualVolume_Quantity.quantity", message.MarketActivityRecord.MarketEvaluationPoint.Series.EstimatedAnnualVolumeQuantity),
                                new XElement(xmlNamespace + "quantity_Measure_Unit.name", message.MarketActivityRecord.MarketEvaluationPoint.Series.QuantityMeasureUnit)),
                            UnitElement(xmlNamespace, "contractedConnectionCapacity", message.MarketActivityRecord.MarketEvaluationPoint.ContractedConnectionCapacity),
                            UnitElement(xmlNamespace, "ratedCurrentConnectionCapacity", message.MarketActivityRecord.MarketEvaluationPoint.RatedCurrent),
                            new XElement(xmlNamespace + "meter.mRID", message.MarketActivityRecord.MarketEvaluationPoint.MeterId),
                            MarketParticipantElement(xmlNamespace, "meteringGridArea_Domain.mRID", message.MarketActivityRecord.MarketEvaluationPoint.EnergySupplierMarketParticipantId),
                            new XElement(xmlNamespace + "supplyStart_DateAndOrTime.dateTime", message.MarketActivityRecord.MarketEvaluationPoint.SupplyStartDateAndOrTimeDateTime),
                            new XElement(xmlNamespace + "description", message.MarketActivityRecord.MarketEvaluationPoint.Description),
                            new XElement(
                                xmlNamespace + "usagePointLocation.mainAddress",
                                new XElement(
                                    xmlNamespace + "streetDetail",
                                    new XElement(xmlNamespace + "number", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.Number),
                                    new XElement(xmlNamespace + "name", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.Name),
                                    new XElement(xmlNamespace + "type", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.Type),
                                    new XElement(xmlNamespace + "code", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.Code),
                                    new XElement(xmlNamespace + "buildingName", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.BuildingName),
                                    new XElement(xmlNamespace + "suiteNumber", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.SuiteNumber),
                                    new XElement(xmlNamespace + "floorIdentification", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.StreetDetail.FloorIdentification)),
                                new XElement(
                                    xmlNamespace + "townDetail",
                                    new XElement(xmlNamespace + "code", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Code),
                                    new XElement(xmlNamespace + "section", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Section),
                                    new XElement(xmlNamespace + "name", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Name),
                                    new XElement(xmlNamespace + "stateOrProvince", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.StateOrProvince),
                                    new XElement(xmlNamespace + "country", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Country)),
                                new XElement(
                                    xmlNamespace + "status",
                                    new XElement(xmlNamespace + "code", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Code),
                                    new XElement(xmlNamespace + "section", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Section),
                                    new XElement(xmlNamespace + "name", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Name),
                                    new XElement(xmlNamespace + "stateOrProvince", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.StateOrProvince)),
                                new XElement(xmlNamespace + "postalCode", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Code),
                                new XElement(xmlNamespace + "poBox", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Code),
                                new XElement(xmlNamespace + "code", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationMainAddress.TownDetail.Code)),
                            new XElement(xmlNamespace + "usagePointLocation.officialAddressIndicator", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationOfficialAddressIndicator),
                            new XElement(xmlNamespace + "usagePointLocation.geoInfoReference", message.MarketActivityRecord.MarketEvaluationPoint.UsagePointLocationGeoInfoReference),
                            MridElement(xmlNamespace, "parent_MarketEvaluationPoint.mRID", message.MarketActivityRecord.MarketEvaluationPoint.ParentMarketEvaluationPointId.Mrid),
                            new XElement(xmlNamespace + "parent_MarketEvaluationPoint.description", message.MarketActivityRecord.MarketEvaluationPoint.ParentMarketEvaluationPointId.Description),
                            MridElement(xmlNamespace, "Child_MarketEvaluationPoint.mRID", message.MarketActivityRecord.MarketEvaluationPoint.ChildMarketEvaluationPoint)))));

            return Serialize(document);
        }

        private static XElement MridElement(XNamespace xmlNamespace, string name, Mrid mrid)
        {
            return new XElement(xmlNamespace + name, new XAttribute("codingScheme", mrid.CodingScheme), mrid.Id);
        }

        private static XElement UnitElement(XNamespace xmlNamespace, string name, UnitValue unitValue)
        {
            return new XElement(xmlNamespace + name, new XAttribute("codingScheme", unitValue.Unit), unitValue.Value);
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
