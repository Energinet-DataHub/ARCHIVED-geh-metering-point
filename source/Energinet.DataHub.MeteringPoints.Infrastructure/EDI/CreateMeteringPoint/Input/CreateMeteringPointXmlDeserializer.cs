﻿// Copyright 2020 Energinet DataHub A/S
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
                    ExtractElementValue(marketEvaluationPoint, ns + "readCycle"),
                    Convert.ToInt32(ExtractElementValue(marketEvaluationPoint, ns + "ratedCurrent")),
                    Convert.ToInt32(ExtractElementValue(contractedConnectionCapacity, ns + "value")),
                    ExtractElementValue(marketEvaluationPoint, ns + "meteringGridArea_Domain.mRID"),
                    ExtractElementValue(linkedMarketEvaluationPoint, ns + "mRID"),
                    ExtractElementValue(marketEvaluationPoint, ns + "usagePointLocation.remark"),
                    ExtractElementValue(marketEvaluationPoint, ns + "product"),
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
            switch (id)
            {
                case MeteringPointType.Consumption: return "Consumption";
                default: return string.Empty;
            }
        }

        private static string GetMeteringPointSubType(string id)
        {
            switch (id)
            {
                case MeteringPointSubType.Physical: return "Physical";
                default: return string.Empty;
            }
        }

        private static string GetConnectionState(string id)
        {
            switch (id)
            {
                case ConnectionState.New: return "New";
                default: return string.Empty;
            }
        }

        private static string GetConnectionType(string id)
        {
            switch (id)
            {
                case ConnectionType.DirectConnected: return "DirectConnected";
                default: return string.Empty;
            }
        }

        private static string GetAssetType(string id)
        {
            switch (id)
            {
                case AssetType.WindTurbines: return "WindTurbines";
                default: return string.Empty;
            }
        }

        private static string GetDisconnectionType(string id)
        {
            switch (id)
            {
                case DisconnectionType.RemoteDisconnection: return "RemoteDisconnection";
                default: return string.Empty;
            }
        }

        private static string GetSettlementMethod(string id)
        {
            switch (id)
            {
                case SettlementMethod.Flex: return "Flex";
                case SettlementMethod.NonProfiled: return "NonProfiled";
                default: return string.Empty;
            }
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
            public const string DirectConnected = "D01";
        }

        private static class AssetType
        {
            public const string WindTurbines = "D12";
        }

        private static class DisconnectionType
        {
            public const string RemoteDisconnection = "D01";
        }
    }
}
