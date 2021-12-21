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
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint
{
    internal static class MeteringPointCreatedIntegrationEventExtension
    {
        public static MeteringPointCreated.Types.MeteringPointType GetMeteringPointType(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.MeteringPointType switch
            {
                "Consumption" => MeteringPointCreated.Types.MeteringPointType.MptConsumption,
                "Production" => MeteringPointCreated.Types.MeteringPointType.MptProduction,
                "Exchange" => MeteringPointCreated.Types.MeteringPointType.MptExchange,
                "GridLossCorrection" => MeteringPointCreated.Types.MeteringPointType.MptGridLossCorrection,
                "Analysis" => MeteringPointCreated.Types.MeteringPointType.MptAnalysis,
                "VEProduction" => MeteringPointCreated.Types.MeteringPointType.MptVeproduction,
                "ExchangeReactiveEnergy" => MeteringPointCreated.Types.MeteringPointType.MptExchangeReactiveEnergy,
                "InternalUse" => MeteringPointCreated.Types.MeteringPointType.MptInternalUse,
                "SurplusProductionGroup" => MeteringPointCreated.Types.MeteringPointType.MptSurplusProductionGroup,
                "NetProduction" => MeteringPointCreated.Types.MeteringPointType.MptNetProduction,
                "SupplyToGrid" => MeteringPointCreated.Types.MeteringPointType.MptSupplyToGrid,
                "ConsumptionFromGrid" => MeteringPointCreated.Types.MeteringPointType.MptConsumptionFromGrid,
                "WholesaleServices" => MeteringPointCreated.Types.MeteringPointType.MptWholesaleServices,
                "OwnProduction" => MeteringPointCreated.Types.MeteringPointType.MptOwnProduction,
                "NetFromGrid" => MeteringPointCreated.Types.MeteringPointType.MptNetFromGrid,
                "NetToGrid" => MeteringPointCreated.Types.MeteringPointType.MptNetToGrid,
                "TotalConsumption" => MeteringPointCreated.Types.MeteringPointType.MptTotalConsumption,
                "ElectricalHeating" => MeteringPointCreated.Types.MeteringPointType.MptElectricalHeating,
                "NetConsumption" => MeteringPointCreated.Types.MeteringPointType.MptNetConsumption,
                "OtherConsumption" => MeteringPointCreated.Types.MeteringPointType.MptOtherConsumption,
                "OtherProduction" => MeteringPointCreated.Types.MeteringPointType.MptOtherProduction,
                _ => throw new ArgumentException("Product type is not recognized."),
            };
        }

        public static MeteringPointCreated.Types.SettlementMethod GetSettlementMethod(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.SettlementMethod switch
            {
                "Flex" => MeteringPointCreated.Types.SettlementMethod.SmFlex,
                "NonProfiled" => MeteringPointCreated.Types.SettlementMethod.SmNonprofiled,
                "Profiled" => MeteringPointCreated.Types.SettlementMethod.SmProfiled,
                _ => throw new ArgumentException("Settlement method is not recognized."),
            };
        }

        public static MeteringPointCreated.Types.NetSettlementGroup GetNetSettlementGroup(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.NetSettlementGroup switch
            {
                "Zero" => MeteringPointCreated.Types.NetSettlementGroup.NsgZero,
                "One" => MeteringPointCreated.Types.NetSettlementGroup.NsgOne,
                "Two" => MeteringPointCreated.Types.NetSettlementGroup.NsgTwo,
                "Three" => MeteringPointCreated.Types.NetSettlementGroup.NsgThree,
                "Six" => MeteringPointCreated.Types.NetSettlementGroup.NsgSix,
                "Ninetynine" => MeteringPointCreated.Types.NetSettlementGroup.NsgNinetynine,
                _ => throw new ArgumentException("Net settlement group is not recognized."),
            };
        }

        public static MeteringPointCreated.Types.ProductType GetProductType(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.Product switch
            {
                "Tariff" => MeteringPointCreated.Types.ProductType.PtTariff,
                "FuelQuantity" => MeteringPointCreated.Types.ProductType.PtFuelquantity,
                "PowerActive" => MeteringPointCreated.Types.ProductType.PtPoweractive,
                "PowerReactive" => MeteringPointCreated.Types.ProductType.PtPowerreactive,
                "EnergyActive" => MeteringPointCreated.Types.ProductType.PtEnergyactive,
                "EnergyReactive" => MeteringPointCreated.Types.ProductType.PtEnergyreactive,
                _ => throw new ArgumentException("Product type is not recognized."),
            };
        }

        public static MeteringPointCreated.Types.MeteringMethod GetMeteringMethod(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.MeteringMethod switch
            {
                "Physical" => MeteringPointCreated.Types.MeteringMethod.MmPhysical,
                "Virtual" => MeteringPointCreated.Types.MeteringMethod.MmVirtual,
                "Calculated" => MeteringPointCreated.Types.MeteringMethod.MmCalculated,
                _ => throw new ArgumentException("Metering method is not recognized."),
            };
        }

        public static MeteringPointCreated.Types.MeterReadingPeriodicity GetMeterReadingPeriodicity(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.MeterReadingPeriodicity switch
            {
                "Hourly" => MeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly,
                "Quarterly" => MeteringPointCreated.Types.MeterReadingPeriodicity.MrpQuarterly,
                _ => throw new ArgumentException("Meter reading periodicity is not recognized."),
            };
        }

        public static MeteringPointCreated.Types.ConnectionState GetConnectionState(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.ConnectionState switch
            {
                "New" => MeteringPointCreated.Types.ConnectionState.CsNew,
                _ => throw new ArgumentException("Meter reading periodicity is not recognized."),
            };
        }

        public static MeteringPointCreated.Types.UnitType GetUnitType(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.UnitType switch
            {
                "Wh" => MeteringPointCreated.Types.UnitType.UtWh,
                "KWh" => MeteringPointCreated.Types.UnitType.UtKwh,
                "MWh" => MeteringPointCreated.Types.UnitType.UtMwh,
                "GWh" => MeteringPointCreated.Types.UnitType.UtGwh,
                _ => throw new ArgumentException("Meter reading periodicity is not recognized."),
            };
        }
    }
}
