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
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Energinet.DataHub.MeteringPoints.Application;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.CreateMeteringPoint.Input
{
    public static class CreateMeteringPointXmlDeserializer
    {
        public static IEnumerable<Application.CreateMeteringPoint> Deserialize(Stream data)
        {
            var root = XElement.Load(data);
            XNamespace ns = "urn:ediel:org:requestchangeofapcharacteristics:0:1";

            var marketActivityRecords = root
                .Elements(ns + "MktActivityRecord");

            return marketActivityRecords.Select(record =>
            {
                var marketEvaluationPoint = record.Element(ns + "MarketEvaluationPoint");
                var mainAddress = marketEvaluationPoint?.Element(ns + "usagePointLocation.mainAddress");
                var streetDetail = mainAddress?.Element(ns + "streetDetail");
                var townDetail = mainAddress?.Element(ns + "townDetail");
                var contractedConnectionCapacity =
                    marketEvaluationPoint?.Element(ns +
                                                   "marketAgreement.contractedConnectionCapacity"); // TODO: MarketAgreement prefix isn't 100% set in stone. Update at a later point.
                var series = marketEvaluationPoint?.Element(ns + "Series");

                // Power plant
                var linkedMarketEvaluationPoint = marketEvaluationPoint?.Element(ns + "Linked_MarketEvaluationPoint");

                var address = new Address(
                    ExtractElementValue(streetDetail, ns + "name"),
                    ExtractElementValue(mainAddress, ns + "postalCode"),
                    ExtractElementValue(townDetail, ns + "name"),
                    ExtractElementValue(townDetail, ns + "country"),
                    ExtractElementValue(mainAddress, ns + "usagePointLocation.remark") == "D01");

                return new Application.CreateMeteringPoint(
                    address,
                    ExtractElementValue(marketEvaluationPoint, ns + "mRID"),
                    GetMeteringPointType(ExtractElementValue(marketEvaluationPoint, ns + "type")),
                    GetMeteringPointSubType(ExtractElementValue(marketEvaluationPoint, ns + "meteringMethod")),
                    GetMeterReadingOccurrence(ExtractElementValue(marketEvaluationPoint, ns + "readCycle")),
                    Convert.ToInt32(ExtractElementValue(marketEvaluationPoint, ns + "ratedCurrent")),
                    Convert.ToInt32(ExtractElementValue(contractedConnectionCapacity, ns + "value")),
                    ExtractElementValue(marketEvaluationPoint, ns + "meteringGridArea_Domain.mRID"),
                    ExtractElementValue(linkedMarketEvaluationPoint, ns + "mRID"),
                    ExtractElementValue(marketEvaluationPoint, ns + "usagePointLocation.remark"),
                    ExtractElementValue(marketEvaluationPoint, ns + "parent_MarketEvaluationPoint.mRID"),
                    GetSettlementMethod(ExtractElementValue(marketEvaluationPoint, ns + "settlementMethod")),
                    ExtractElementValue(series, ns + "quantity_Measure_Unit.name"),
                    GetDisconnectionType(ExtractElementValue(marketEvaluationPoint, ns + "disconnectionMethod")),
                    ExtractElementValue(record, ns + "start_DateAndOrTime.dateTime"),
                    ExtractElementValue(marketEvaluationPoint, ns + "meter.mRID"),
                    ExtractElementValue(record, ns + "mRID"),
                    GetConnectionState(ExtractElementValue(marketEvaluationPoint, ns + "connectionState")),
                    ExtractElementValue(marketEvaluationPoint, ns + "netSettlementGroup"),
                    GetConnectionType(ExtractElementValue(marketEvaluationPoint, ns + "mPConnectionType")),
                    GetAssetType(ExtractElementValue(marketEvaluationPoint, ns + "energyLabel_EnergyTechnologyAndFuel.technology")));
            });
        }

        private static string GetMeteringPointType(string id)
        {
            return id switch
            {
                MeteringPointType.Consumption => nameof(MeteringPointType.Consumption),
                _ => string.Empty,
            };
        }

        private static string GetMeteringPointSubType(string id)
        {
            return id switch
            {
                MeteringPointSubType.Physical => nameof(MeteringPointSubType.Physical),
                _ => string.Empty,
            };
        }

        private static string GetConnectionState(string id)
        {
            return id switch
            {
                ConnectionState.New => nameof(ConnectionState.New),
                _ => string.Empty,
            };
        }

        private static string GetConnectionType(string id)
        {
            return id switch
            {
                ConnectionType.Direct => nameof(ConnectionType.Direct),
                ConnectionType.Installation => nameof(ConnectionType.Installation),
                _ => string.Empty,
            };
        }

        private static string GetAssetType(string id)
        {
            return id switch
            {
                AssetType.WindTurbines => nameof(AssetType.WindTurbines),
                _ => string.Empty,
            };
        }

        private static string GetDisconnectionType(string id)
        {
            return id switch
            {
                DisconnectionType.Remote => nameof(DisconnectionType.Remote),
                DisconnectionType.Manual => nameof(DisconnectionType.Manual),
                _ => string.Empty,
            };
        }

        private static string GetSettlementMethod(string id)
        {
            return id switch
            {
                SettlementMethod.Flex => nameof(SettlementMethod.Flex),
                SettlementMethod.NonProfiled => nameof(SettlementMethod.NonProfiled),
                _ => string.Empty,
            };
        }

        private static string GetMeterReadingOccurrence(string value)
        {
            return value switch
            {
                ReadingOccurrence.Yearly => nameof(ReadingOccurrence.Yearly),
                ReadingOccurrence.Monthly => nameof(ReadingOccurrence.Monthly),
                ReadingOccurrence.Hourly => nameof(ReadingOccurrence.Hourly),
                ReadingOccurrence.Quarterly => nameof(ReadingOccurrence.Quarterly),
                _ => string.Empty,
            };
        }

        private static string ExtractElementValue(XElement? element, XName name)
        {
            return element?.Element(name)?.Value ?? string.Empty;
        }

        private static class MeteringPointType
        {
            public const string Consumption = "E17";
        }

        private static class SettlementMethod
        {
            public const string Flex = "D01";
            public const string NonProfiled = "E02";
        }

        private static class ReadingOccurrence
        {
            public const string Yearly = "P1Y";
            public const string Monthly = "P1M";
            public const string Hourly = "PT1H";
            public const string Quarterly = "PT15M";
        }

        private static class MeteringPointSubType
        {
            public const string Physical = "D01";
        }

        private static class ConnectionState
        {
            public const string New = "D03";
        }

        private static class ConnectionType
        {
            public const string Direct = "D01";
            public const string Installation = "D02";
        }

        private static class AssetType
        {
            public const string WindTurbines = "D12";
        }

        private static class DisconnectionType
        {
            public const string Remote = "D01";
            public const string Manual = "D02";
        }
    }
}
