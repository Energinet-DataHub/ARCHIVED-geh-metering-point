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

using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter.Mappings
{
    public class CreateMeteringPointXmlMappingConfiguration : XmlMappingConfigurationBase
    {
        public CreateMeteringPointXmlMappingConfiguration()
        {
            CreateMapping<Application.CreateMeteringPoint>("MktActivityRecord", mapper => mapper
                .AddProperty(x => x.GsrnNumber, "MarketEvaluationPoint", "mRID")
                .AddProperty(x => x.AssetType, TranslateAssetType, "MarketEvaluationPoint", "asset_MktPSRType.psrType")
                .AddProperty(x => x.MaximumPower, "MarketEvaluationPoint", "contractedConnectionCapacity")
                .AddProperty(x => x.MaximumCurrent, "MarketEvaluationPoint", "ratedCurrent")
                .AddProperty(x => x.TypeOfMeteringPoint, TranslateMeteringPointType, "MarketEvaluationPoint", "type")
                .AddProperty(x => x.SubTypeOfMeteringPoint, TranslateMeteringPointSubType, "MarketEvaluationPoint", "meteringMethod")
                .AddProperty(x => x.MeterReadingOccurrence, TranslateMeterReadingOccurrence, "MarketEvaluationPoint", "readCycle")
                .AddProperty(x => x.MeteringGridArea, "MarketEvaluationPoint", "meteringGridArea_Domain.mRID")
                .AddProperty(x => x.PowerPlant, "MarketEvaluationPoint", "Linked_MarketEvaluationPoint", "mRID")
                .AddProperty(x => x.LocationDescription, "MarketEvaluationPoint", "description")
                .AddProperty(x => x.SettlementMethod, TranslateSettlementMethod, "MarketEvaluationPoint", "settlementMethod")
                .AddProperty(x => x.UnitType, "MarketEvaluationPoint", "Series", "quantity_Measure_Unit.name")
                .AddProperty(x => x.DisconnectionType, TranslateDisconnectionType, "MarketEvaluationPoint", "disconnectionMethod")
                .AddProperty(x => x.OccurenceDate, "validityStart_DateAndOrTime.dateTime")
                .AddProperty(x => x.MeterNumber, "MarketEvaluationPoint", "meter.mRID")
                .AddProperty(x => x.TransactionId, "mRID")
                .AddProperty(x => x.PhysicalStatusOfMeteringPoint, TranslatePhysicalState, "MarketEvaluationPoint", "connectionState")
                .AddProperty(x => x.NetSettlementGroup, TranslateNetSettlementGroup, "MarketEvaluationPoint", "netSettlementGroup")
                .AddProperty(x => x.ConnectionType, TranslateConnectionType, "MarketEvaluationPoint", "mPConnectionType")
                .AddProperty(x => x.StreetName, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "streetDetail", "name")
                .AddProperty(x => x.CityName, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "townDetail", "name")
                .AddProperty(x => x.PostCode, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "postalCode")
                .AddProperty(x => x.StreetCode, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "streetDetail", "code")
                .AddProperty(x => x.CountryCode, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "townDetail", "country")
                .AddProperty(x => x.FloorIdentification, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "streetDetail", "floorIdentification")
                .AddProperty(x => x.RoomIdentification, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "streetDetail", "suiteNumber")
                .AddProperty(x => x.IsWashable, IsWashable, "MarketEvaluationPoint", "usagePointLocation.officialAddressIndicator")
                .AddProperty(x => x.FromGrid, "MarketEvaluationPoint", "inMeteringGridArea_Domain.mRID")
                .AddProperty(x => x.ToGrid, "MarketEvaluationPoint", "outMeteringGridArea_Domain.mRID")
                .AddProperty(x => x.ParentRelatedMeteringPoint, "MarketEvaluationPoint", "parent_MarketEvaluationPoint.mRID"));
        }

        private static bool IsWashable(XmlElementInfo remark)
        {
            return bool.TryParse(remark.SourceValue, out var b) && b;
        }

        private static string TranslateSettlementMethod(XmlElementInfo settlementMethod)
        {
            return settlementMethod.SourceValue switch
            {
                "D01" => nameof(SettlementMethod.Flex),
                "E02" => nameof(SettlementMethod.NonProfiled),
                _ => string.Empty,
            };
        }

        private static string TranslateNetSettlementGroup(XmlElementInfo netSettlementGroup)
        {
            return netSettlementGroup.SourceValue switch
            {
                "0" => nameof(NetSettlementGroup.Zero),
                "1" => nameof(NetSettlementGroup.One),
                "2" => nameof(NetSettlementGroup.Two),
                "3" => nameof(NetSettlementGroup.Three),
                "6" => nameof(NetSettlementGroup.Six),
                "99" => nameof(NetSettlementGroup.Ninetynine),
                _ => string.Empty,
            };
        }

        private static string TranslateMeteringPointType(XmlElementInfo meteringPointType)
        {
            return meteringPointType.SourceValue switch
            {
                "E17" => nameof(MeteringPointType.Consumption),
                "E18" => nameof(MeteringPointType.Production),
                "E20" => nameof(MeteringPointType.Exchange),
                _ => string.Empty,
            };
        }

        private static string TranslateMeteringPointSubType(XmlElementInfo meteringPointSubType)
        {
            return meteringPointSubType.SourceValue switch
            {
                "D01" => nameof(MeteringPointSubType.Physical),
                "D02" => nameof(MeteringPointSubType.Virtual),
                "D03" => nameof(MeteringPointSubType.Calculated),
                _ => string.Empty,
            };
        }

        private static string TranslatePhysicalState(XmlElementInfo physicalState)
        {
            return physicalState.SourceValue switch
            {
                "D03" => nameof(PhysicalState.New),
                _ => string.Empty,
            };
        }

        private static string TranslateConnectionType(XmlElementInfo connectionType)
        {
            return connectionType.SourceValue switch
            {
                "D01" => nameof(ConnectionType.Direct),
                "D02" => nameof(ConnectionType.Installation),
                _ => string.Empty,
            };
        }

        private static string TranslateAssetType(XmlElementInfo assetType)
        {
            return assetType.SourceValue switch
            {
                "D12" => nameof(AssetType.WindTurbines),
                _ => string.Empty,
            };
        }

        private static string TranslateDisconnectionType(XmlElementInfo disconnectionType)
        {
            return disconnectionType.SourceValue switch
            {
                "D01" => nameof(DisconnectionType.Remote),
                "D02" => nameof(DisconnectionType.Manual),
                _ => string.Empty,
            };
        }

        private static string TranslateMeterReadingOccurrence(XmlElementInfo meterReadingOccurrence)
        {
            return meterReadingOccurrence.SourceValue switch
            {
                "P1Y" => nameof(ReadingOccurrence.Yearly),
                "P1M" => nameof(ReadingOccurrence.Monthly),
                "PT1H" => nameof(ReadingOccurrence.Hourly),
                "PT15M" => nameof(ReadingOccurrence.Quarterly),
                _ => string.Empty,
            };
        }
    }
}
