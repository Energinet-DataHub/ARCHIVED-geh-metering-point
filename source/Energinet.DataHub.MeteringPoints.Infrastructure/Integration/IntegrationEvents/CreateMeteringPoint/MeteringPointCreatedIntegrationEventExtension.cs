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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
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
                nameof(MeteringPointType.Consumption) => MeteringPointCreated.Types.MeteringPointType.MptConsumption,
                nameof(MeteringPointType.Production) => MeteringPointCreated.Types.MeteringPointType.MptProduction,
                nameof(MeteringPointType.Exchange) => MeteringPointCreated.Types.MeteringPointType.MptExchange,
                nameof(MeteringPointType.GridLossCorrection) => MeteringPointCreated.Types.MeteringPointType.MptGridLossCorrection,
                nameof(MeteringPointType.Analysis) => MeteringPointCreated.Types.MeteringPointType.MptAnalysis,
                nameof(MeteringPointType.VEProduction) => MeteringPointCreated.Types.MeteringPointType.MptVeproduction,
                nameof(MeteringPointType.ExchangeReactiveEnergy) => MeteringPointCreated.Types.MeteringPointType.MptExchangeReactiveEnergy,
                nameof(MeteringPointType.InternalUse) => MeteringPointCreated.Types.MeteringPointType.MptInternalUse,
                nameof(MeteringPointType.SurplusProductionGroup) => MeteringPointCreated.Types.MeteringPointType.MptSurplusProductionGroup,
                nameof(MeteringPointType.NetProduction) => MeteringPointCreated.Types.MeteringPointType.MptNetProduction,
                nameof(MeteringPointType.SupplyToGrid) => MeteringPointCreated.Types.MeteringPointType.MptSupplyToGrid,
                nameof(MeteringPointType.ConsumptionFromGrid) => MeteringPointCreated.Types.MeteringPointType.MptConsumptionFromGrid,
                nameof(MeteringPointType.WholesaleServices) => MeteringPointCreated.Types.MeteringPointType.MptWholesaleServices,
                nameof(MeteringPointType.OwnProduction) => MeteringPointCreated.Types.MeteringPointType.MptOwnProduction,
                nameof(MeteringPointType.NetFromGrid) => MeteringPointCreated.Types.MeteringPointType.MptNetFromGrid,
                nameof(MeteringPointType.NetToGrid) => MeteringPointCreated.Types.MeteringPointType.MptNetToGrid,
                nameof(MeteringPointType.TotalConsumption) => MeteringPointCreated.Types.MeteringPointType.MptTotalConsumption,
                nameof(MeteringPointType.ElectricalHeating) => MeteringPointCreated.Types.MeteringPointType.MptElectricalHeating,
                nameof(MeteringPointType.NetConsumption) => MeteringPointCreated.Types.MeteringPointType.MptNetConsumption,
                nameof(MeteringPointType.OtherConsumption) => MeteringPointCreated.Types.MeteringPointType.MptOtherConsumption,
                nameof(MeteringPointType.OtherProduction) => MeteringPointCreated.Types.MeteringPointType.MptOtherProduction,
                _ => MeteringPointCreated.Types.MeteringPointType.MptUnknown,
            };
        }

        public static MeteringPointCreated.Types.SettlementMethod GetSettlementMethod(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.SettlementMethod switch
            {
                nameof(SettlementMethod.Flex) => MeteringPointCreated.Types.SettlementMethod.SmFlex,
                nameof(SettlementMethod.NonProfiled) => MeteringPointCreated.Types.SettlementMethod.SmNonprofiled,
                nameof(SettlementMethod.Profiled) => MeteringPointCreated.Types.SettlementMethod.SmProfiled,
                _ => MeteringPointCreated.Types.SettlementMethod.SmUnknown,
            };
        }

        public static MeteringPointCreated.Types.NetSettlementGroup GetNetSettlementGroup(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.NetSettlementGroup switch
            {
                nameof(NetSettlementGroup.Zero) => MeteringPointCreated.Types.NetSettlementGroup.NsgZero,
                nameof(NetSettlementGroup.One) => MeteringPointCreated.Types.NetSettlementGroup.NsgOne,
                nameof(NetSettlementGroup.Two) => MeteringPointCreated.Types.NetSettlementGroup.NsgTwo,
                nameof(NetSettlementGroup.Three) => MeteringPointCreated.Types.NetSettlementGroup.NsgThree,
                nameof(NetSettlementGroup.Six) => MeteringPointCreated.Types.NetSettlementGroup.NsgSix,
                nameof(NetSettlementGroup.Ninetynine) => MeteringPointCreated.Types.NetSettlementGroup.NsgNinetynine,
                _ => MeteringPointCreated.Types.NetSettlementGroup.NsgUnknown,
            };
        }

        public static MeteringPointCreated.Types.ProductType GetProductType(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.Product switch
            {
                nameof(ProductType.Tariff) => MeteringPointCreated.Types.ProductType.PtTariff,
                nameof(ProductType.FuelQuantity) => MeteringPointCreated.Types.ProductType.PtFuelquantity,
                nameof(ProductType.PowerActive) => MeteringPointCreated.Types.ProductType.PtPoweractive,
                nameof(ProductType.PowerReactive) => MeteringPointCreated.Types.ProductType.PtPowerreactive,
                nameof(ProductType.EnergyActive) => MeteringPointCreated.Types.ProductType.PtEnergyactive,
                nameof(ProductType.EnergyReactive) => MeteringPointCreated.Types.ProductType.PtEnergyreactive,
                _ => MeteringPointCreated.Types.ProductType.PtUnknown,
            };
        }

        public static MeteringPointCreated.Types.MeteringMethod GetMeteringMethod(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.MeteringMethod switch
            {
                nameof(MeteringMethod.Physical) => MeteringPointCreated.Types.MeteringMethod.MmPhysical,
                nameof(MeteringMethod.Virtual) => MeteringPointCreated.Types.MeteringMethod.MmVirtual,
                nameof(MeteringMethod.Calculated) => MeteringPointCreated.Types.MeteringMethod.MmCalculated,
                _ => MeteringPointCreated.Types.MeteringMethod.MmUnknown,
            };
        }

        public static MeteringPointCreated.Types.MeterReadingPeriodicity GetMeterReadingPeriodicity(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.MeterReadingPeriodicity switch
            {
                nameof(ReadingOccurrence.Hourly) => MeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly,
                nameof(ReadingOccurrence.Quarterly) => MeteringPointCreated.Types.MeterReadingPeriodicity.MrpQuarterly,
                _ => MeteringPointCreated.Types.MeterReadingPeriodicity.MrpUnknown,
            };
        }

        public static MeteringPointCreated.Types.ConnectionState GetConnectionState(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.ConnectionState switch
            {
                nameof(ConnectionState.New) => MeteringPointCreated.Types.ConnectionState.CsNew,
                _ => MeteringPointCreated.Types.ConnectionState.CsUnknown,
            };
        }

        public static MeteringPointCreated.Types.UnitType GetUnitType(
            this MeteringPointCreatedEventMessage @event)
        {
            return @event.UnitType switch
            {
                nameof(MeasurementUnitType.Wh) => MeteringPointCreated.Types.UnitType.UtWh,
                nameof(MeasurementUnitType.KWh) => MeteringPointCreated.Types.UnitType.UtKwh,
                nameof(MeasurementUnitType.MWh) => MeteringPointCreated.Types.UnitType.UtMwh,
                nameof(MeasurementUnitType.GWh) => MeteringPointCreated.Types.UnitType.UtGwh,
                _ => MeteringPointCreated.Types.UnitType.UtUnknown,
            };
        }
    }
}
