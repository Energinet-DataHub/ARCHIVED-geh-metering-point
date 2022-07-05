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

using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using masterData = Energinet.DataHub.MeteringPoints.IntegrationEvents.ChangeMasterData;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeMasterData.MasterDataUpdated
{
    internal static class MasterDataUpdatedIntegrationEventExtension
    {
        public static masterData.MasterDataUpdated.Types.SettlementMethod GetSettlementMethod(
            this MasterDataWasUpdatedIntegrationEvent @event)
        {
            return @event.SettlementMethod switch
            {
                nameof(SettlementMethod.Flex) => masterData.MasterDataUpdated.Types.SettlementMethod.SmFlex,
                nameof(SettlementMethod.NonProfiled) => masterData.MasterDataUpdated.Types.SettlementMethod.SmNonprofiled,
                nameof(SettlementMethod.Profiled) => masterData.MasterDataUpdated.Types.SettlementMethod.SmProfiled,
                _ => masterData.MasterDataUpdated.Types.SettlementMethod.SmNull,
            };
        }

        public static masterData.MasterDataUpdated.Types.NetSettlementGroup GetNetSettlementGroup(
            this MasterDataWasUpdatedIntegrationEvent @event)
        {
            return @event.NetSettlementGroup switch
            {
                nameof(NetSettlementGroup.Zero) => masterData.MasterDataUpdated.Types.NetSettlementGroup.NsgZero,
                nameof(NetSettlementGroup.One) => masterData.MasterDataUpdated.Types.NetSettlementGroup.NsgOne,
                nameof(NetSettlementGroup.Two) => masterData.MasterDataUpdated.Types.NetSettlementGroup.NsgTwo,
                nameof(NetSettlementGroup.Three) => masterData.MasterDataUpdated.Types.NetSettlementGroup.NsgThree,
                nameof(NetSettlementGroup.Six) => masterData.MasterDataUpdated.Types.NetSettlementGroup.NsgSix,
                nameof(NetSettlementGroup.Ninetynine) => masterData.MasterDataUpdated.Types.NetSettlementGroup.NsgNinetynine,
                _ => masterData.MasterDataUpdated.Types.NetSettlementGroup.NsgNull,
            };
        }

        public static masterData.MasterDataUpdated.Types.ProductType GetProductType(
            this MasterDataWasUpdatedIntegrationEvent @event)
        {
            return @event.ProductType switch
            {
                nameof(ProductType.Tariff) => masterData.MasterDataUpdated.Types.ProductType.PtTariff,
                nameof(ProductType.FuelQuantity) => masterData.MasterDataUpdated.Types.ProductType.PtFuelquantity,
                nameof(ProductType.PowerActive) => masterData.MasterDataUpdated.Types.ProductType.PtPoweractive,
                nameof(ProductType.PowerReactive) => masterData.MasterDataUpdated.Types.ProductType.PtPowerreactive,
                nameof(ProductType.EnergyActive) => masterData.MasterDataUpdated.Types.ProductType.PtEnergyactive,
                nameof(ProductType.EnergyReactive) => masterData.MasterDataUpdated.Types.ProductType.PtEnergyreactive,
                _ => masterData.MasterDataUpdated.Types.ProductType.PtNull,
            };
        }

        public static masterData.MasterDataUpdated.Types.MeteringMethod GetMeteringMethod(
            this MasterDataWasUpdatedIntegrationEvent @event)
        {
            return @event.MeteringMethod switch
            {
                nameof(MeteringMethod.Physical) => masterData.MasterDataUpdated.Types.MeteringMethod.MmPhysical,
                nameof(MeteringMethod.Virtual) => masterData.MasterDataUpdated.Types.MeteringMethod.MmVirtual,
                nameof(MeteringMethod.Calculated) => masterData.MasterDataUpdated.Types.MeteringMethod.MmCalculated,
                _ => masterData.MasterDataUpdated.Types.MeteringMethod.MmNull,
            };
        }

        public static masterData.MasterDataUpdated.Types.MeterReadingPeriodicity GetMeterReadingPeriodicity(
            this MasterDataWasUpdatedIntegrationEvent @event)
        {
            return @event.ReadingOccurrence switch
            {
                nameof(ReadingOccurrence.Hourly) => masterData.MasterDataUpdated.Types.MeterReadingPeriodicity.MrpHourly,
                nameof(ReadingOccurrence.Quarterly) => masterData.MasterDataUpdated.Types.MeterReadingPeriodicity.MrpQuarterly,
                nameof(ReadingOccurrence.Monthly) => masterData.MasterDataUpdated.Types.MeterReadingPeriodicity.MrpMonthly,
                nameof(ReadingOccurrence.Yearly) => masterData.MasterDataUpdated.Types.MeterReadingPeriodicity.MrpYearly,
                _ => masterData.MasterDataUpdated.Types.MeterReadingPeriodicity.MrpNull,
            };
        }

        public static masterData.MasterDataUpdated.Types.UnitType GetUnitType(
            this MasterDataWasUpdatedIntegrationEvent @event)
        {
            return @event.UnitType switch
            {
                nameof(MeasurementUnitType.Wh) => masterData.MasterDataUpdated.Types.UnitType.UtWh,
                nameof(MeasurementUnitType.KWh) => masterData.MasterDataUpdated.Types.UnitType.UtKwh,
                nameof(MeasurementUnitType.MWh) => masterData.MasterDataUpdated.Types.UnitType.UtMwh,
                nameof(MeasurementUnitType.GWh) => masterData.MasterDataUpdated.Types.UnitType.UtGwh,
                _ => masterData.MasterDataUpdated.Types.UnitType.UtNull,
            };
        }
    }
}
