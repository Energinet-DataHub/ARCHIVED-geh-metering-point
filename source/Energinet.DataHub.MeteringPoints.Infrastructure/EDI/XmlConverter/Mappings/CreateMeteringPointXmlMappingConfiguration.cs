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
            CreateMapping<Application.Create.CreateMeteringPoint>("MktActivityRecord", mapper => mapper
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
                .AddProperty(x => x.CityName, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "townDetail", "code")
                .AddProperty(x => x.CitySubDivisionName, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "townDetail", "name")
                .AddProperty(x => x.MunicipalityCode, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "townDetail", "code")
                .AddProperty(x => x.PostCode, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "postalCode")
                .AddProperty(x => x.StreetCode, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "streetDetail", "code")
                .AddProperty(x => x.CountryCode, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "townDetail", "country")
                .AddProperty(x => x.FloorIdentification, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "streetDetail", "floorIdentification")
                .AddProperty(x => x.RoomIdentification, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "streetDetail", "suiteNumber")
                .AddProperty(x => x.BuildingNumber, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "streetDetail", "number")
                .AddProperty(x => x.FromGrid, "MarketEvaluationPoint", "inMeteringGridArea_Domain.mRID")
                .AddProperty(x => x.ToGrid, "MarketEvaluationPoint", "outMeteringGridArea_Domain.mRID")
                .AddProperty(x => x.ParentRelatedMeteringPoint, "MarketEvaluationPoint", "parent_MarketEvaluationPoint.mRID")
                .AddProperty(x => x.ProductType, TranslateProductType, "MarketEvaluationPoint", "Series", "product")
                .AddProperty(x => x.PhysicalConnectionCapacity, "MarketEvaluationPoint", "physicalConnectionCapacity")
                .AddProperty(x => x.IsOfficialAddress, OfficialAddressIndicator, "MarketEvaluationPoint", "usagePointLocation.officialAddressIndicator")
                .AddProperty(x => x.GeoInfoReference, "MarketEvaluationPoint", "usagePointLocation.geoInfoReference")
                .AddProperty(x => x.MeasureUnitType, TranslateMeasureUnitType, "MarketEvaluationPoint", "Series", "quantity_Measure_Unit.name")
                .AddProperty(x => x.ContractedConnectionCapacity, TranslateMeasureUnitType, "MarketEvaluationPoint", "contractedConnectionCapacity")
                .AddProperty(x => x.RatedCurrent, TranslateMeasureUnitType, "MarketEvaluationPoint", "ratedCurrent"));
        }

        private static bool? OfficialAddressIndicator(XmlElementInfo element)
        {
            if (bool.TryParse(element?.SourceValue, out var result))
            {
                return result;
            }

            return null;
        }

        private static string TranslateSettlementMethod(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "D01" => nameof(SettlementMethod.Flex),
                "E02" => nameof(SettlementMethod.NonProfiled),
                "E01" => nameof(SettlementMethod.Profiled),
                _ => element.SourceValue,
            };
        }

        private static string TranslateNetSettlementGroup(XmlElementInfo element)
        {
            return element.SourceValue switch
            {
                "0" => nameof(NetSettlementGroup.Zero),
                "1" => nameof(NetSettlementGroup.One),
                "2" => nameof(NetSettlementGroup.Two),
                "3" => nameof(NetSettlementGroup.Three),
                "6" => nameof(NetSettlementGroup.Six),
                "99" => nameof(NetSettlementGroup.Ninetynine),
                _ => element.SourceValue,
            };
        }

        private static string TranslateMeteringPointType(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "D01" => nameof(MeteringPointType.VEProduction),
                "D02" => nameof(MeteringPointType.Analysis),
                "D20" => nameof(MeteringPointType.ExchangeReactiveEnergy),
                "D99" => nameof(MeteringPointType.InternalUse),
                "E17" => nameof(MeteringPointType.Consumption),
                "E18" => nameof(MeteringPointType.Production),
                "E20" => nameof(MeteringPointType.Exchange),
                "D04" => nameof(MeteringPointType.SurplusProductionGroup),
                "D05" => nameof(MeteringPointType.NetProduction),
                "D06" => nameof(MeteringPointType.SupplyToGrid),
                "D07" => nameof(MeteringPointType.ConsumptionFromGrid),
                "D08" => nameof(MeteringPointType.WholesaleServices),
                "D09" => nameof(MeteringPointType.OwnProduction),
                "D10" => nameof(MeteringPointType.NetFromGrid),
                "D11" => nameof(MeteringPointType.NetToGrid),
                "D12" => nameof(MeteringPointType.TotalConsumption),
                "D13" => nameof(MeteringPointType.GridLossCorrection),
                "D14" => nameof(MeteringPointType.ElectricalHeating),
                "D15" => nameof(MeteringPointType.NetConsumption),
                "D17" => nameof(MeteringPointType.OtherConsumption),
                "D18" => nameof(MeteringPointType.OtherProduction),
                _ => element.SourceValue,
            };
        }

        private static string TranslateMeteringPointSubType(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "D01" => nameof(MeteringPointSubType.Physical),
                "D02" => nameof(MeteringPointSubType.Virtual),
                "D03" => nameof(MeteringPointSubType.Calculated),
                _ => element.SourceValue,
            };
        }

        private static string TranslatePhysicalState(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "D02" => nameof(PhysicalState.ClosedDown),
                "D03" => nameof(PhysicalState.New),
                "E22" => nameof(PhysicalState.Connected),
                "E23" => nameof(PhysicalState.Disconnected),
                _ => element.SourceValue,
            };
        }

        private static string TranslateConnectionType(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "D01" => nameof(ConnectionType.Direct),
                "D02" => nameof(ConnectionType.Installation),
                _ => element.SourceValue,
            };
        }

        private static string TranslateAssetType(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "D01" => nameof(AssetType.SteamTurbineWithBackPressureMode),
                "D02" => nameof(AssetType.GasTurbine),
                "D03" => nameof(AssetType.CombinedCycle),
                "D04" => nameof(AssetType.CombustionEngineGas),
                "D05" => nameof(AssetType.SteamTurbineWithCondensation),
                "D06" => nameof(AssetType.Boiler),
                "D07" => nameof(AssetType.StirlingEngine),
                "D10" => nameof(AssetType.FuelCells),
                "D11" => nameof(AssetType.PhotovoltaicCells),
                "D12" => nameof(AssetType.WindTurbines),
                "D13" => nameof(AssetType.HydroelectricPower),
                "D14" => nameof(AssetType.WavePower),
                "D17" => nameof(AssetType.DispatchableWindTurbines),
                "D19" => nameof(AssetType.DieselCombustionEngine),
                "D20" => nameof(AssetType.BioCombustionEngine),
                "D99" => nameof(AssetType.UnknownTechnology),

                _ => element.SourceValue,
            };
        }

        private static string TranslateDisconnectionType(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "D01" => nameof(DisconnectionType.Remote),
                "D02" => nameof(DisconnectionType.Manual),
                _ => element.SourceValue,
            };
        }

        private static string TranslateMeterReadingOccurrence(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "P1Y" => nameof(ReadingOccurrence.Yearly),
                "P1M" => nameof(ReadingOccurrence.Monthly),
                "PT1H" => nameof(ReadingOccurrence.Hourly),
                "PT15M" => nameof(ReadingOccurrence.Quarterly),
                _ => element.SourceValue,
            };
        }

        private static string TranslateProductType(XmlElementInfo element)
        {
            return element.SourceValue switch
            {
                "8716867000030" => nameof(ProductType.EnergyActive),
                "8716867000047" => nameof(ProductType.EnergyReactive),
                "8716867000016" => nameof(ProductType.PowerActive),
                "8716867000023" => nameof(ProductType.PowerReactive),
                "5790001330606" => nameof(ProductType.FuelQuantity),
                "5790001330590" => nameof(ProductType.Tariff),
                _ => element.SourceValue,
            };
        }

        private static string TranslateMeasureUnitType(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "K3" => nameof(MeasurementUnitType.KVArh),
                "KWH" => nameof(MeasurementUnitType.KWh),
                "KWT" => nameof(MeasurementUnitType.KW),
                "MAW" => nameof(MeasurementUnitType.MW),
                "MWH" => nameof(MeasurementUnitType.MWh),
                "TNE" => nameof(MeasurementUnitType.Tonne),
                "Z03" => nameof(MeasurementUnitType.MVAr),
                "AMP" => nameof(MeasurementUnitType.Ampere),
                "H87" => nameof(MeasurementUnitType.STK),
                "Z14" => nameof(MeasurementUnitType.DanishTariffCode),
                _ => element.SourceValue,
            };
        }
    }
}
