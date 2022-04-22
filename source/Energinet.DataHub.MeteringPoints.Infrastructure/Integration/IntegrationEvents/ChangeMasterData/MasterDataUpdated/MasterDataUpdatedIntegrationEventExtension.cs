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

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.ChangeMasterData.MasterDataUpdated
{
    internal static class MasterDataUpdatedIntegrationEventExtension
    {
        public static IntegrationEventContracts.MasterDataUpdated.Types.SettlementMethod GetSettlementMethod(
            this MasterDataWasUpdatedIntegrationEvent @event)
        {
            return @event.SettlementMethod switch
            {
                nameof(SettlementMethod.Flex) => IntegrationEventContracts.MasterDataUpdated.Types.SettlementMethod.SmFlex,
                nameof(SettlementMethod.NonProfiled) => IntegrationEventContracts.MasterDataUpdated.Types.SettlementMethod.SmNonprofiled,
                nameof(SettlementMethod.Profiled) => IntegrationEventContracts.MasterDataUpdated.Types.SettlementMethod.SmProfiled,
                _ => IntegrationEventContracts.MasterDataUpdated.Types.SettlementMethod.SmNull,
            };
        }

        public static IntegrationEventContracts.MasterDataUpdated.Types.NetSettlementGroup GetNetSettlementGroup(
            this MasterDataWasUpdatedIntegrationEvent @event)
        {
            return @event.NetSettlementGroup switch
            {
                nameof(NetSettlementGroup.Zero) => IntegrationEventContracts.MasterDataUpdated.Types.NetSettlementGroup.NsgZero,
                nameof(NetSettlementGroup.One) => IntegrationEventContracts.MasterDataUpdated.Types.NetSettlementGroup.NsgOne,
                nameof(NetSettlementGroup.Two) => IntegrationEventContracts.MasterDataUpdated.Types.NetSettlementGroup.NsgTwo,
                nameof(NetSettlementGroup.Three) => IntegrationEventContracts.MasterDataUpdated.Types.NetSettlementGroup.NsgThree,
                nameof(NetSettlementGroup.Six) => IntegrationEventContracts.MasterDataUpdated.Types.NetSettlementGroup.NsgSix,
                nameof(NetSettlementGroup.Ninetynine) => IntegrationEventContracts.MasterDataUpdated.Types.NetSettlementGroup.NsgNinetynine,
                _ => IntegrationEventContracts.MasterDataUpdated.Types.NetSettlementGroup.NsgNull,
            };
        }

        public static IntegrationEventContracts.MasterDataUpdated.Types.ProductType GetProductType(
            this MasterDataWasUpdatedIntegrationEvent @event)
        {
            return @event.ProductType switch
            {
                nameof(ProductType.Tariff) => IntegrationEventContracts.MasterDataUpdated.Types.ProductType.PtTariff,
                nameof(ProductType.FuelQuantity) => IntegrationEventContracts.MasterDataUpdated.Types.ProductType.PtFuelquantity,
                nameof(ProductType.PowerActive) => IntegrationEventContracts.MasterDataUpdated.Types.ProductType.PtPoweractive,
                nameof(ProductType.PowerReactive) => IntegrationEventContracts.MasterDataUpdated.Types.ProductType.PtPowerreactive,
                nameof(ProductType.EnergyActive) => IntegrationEventContracts.MasterDataUpdated.Types.ProductType.PtEnergyactive,
                nameof(ProductType.EnergyReactive) => IntegrationEventContracts.MasterDataUpdated.Types.ProductType.PtEnergyreactive,
                _ => IntegrationEventContracts.MasterDataUpdated.Types.ProductType.PtNull,
            };
        }

        public static IntegrationEventContracts.MasterDataUpdated.Types.MeteringMethod GetMeteringMethod(
            this MasterDataWasUpdatedIntegrationEvent @event)
        {
            return @event.MeteringMethod switch
            {
                nameof(MeteringMethod.Physical) => IntegrationEventContracts.MasterDataUpdated.Types.MeteringMethod.MmPhysical,
                nameof(MeteringMethod.Virtual) => IntegrationEventContracts.MasterDataUpdated.Types.MeteringMethod.MmVirtual,
                nameof(MeteringMethod.Calculated) => IntegrationEventContracts.MasterDataUpdated.Types.MeteringMethod.MmCalculated,
                _ => IntegrationEventContracts.MasterDataUpdated.Types.MeteringMethod.MmNull,
            };
        }

        public static IntegrationEventContracts.MasterDataUpdated.Types.MeterReadingPeriodicity GetMeterReadingPeriodicity(
            this MasterDataWasUpdatedIntegrationEvent @event)
        {
            return @event.ReadingOccurrence switch
            {
                nameof(ReadingOccurrence.Hourly) => IntegrationEventContracts.MasterDataUpdated.Types.MeterReadingPeriodicity.MrpHourly,
                nameof(ReadingOccurrence.Quarterly) => IntegrationEventContracts.MasterDataUpdated.Types.MeterReadingPeriodicity.MrpQuarterly,
                nameof(ReadingOccurrence.Monthly) => IntegrationEventContracts.MasterDataUpdated.Types.MeterReadingPeriodicity.MrpMonthly,
                nameof(ReadingOccurrence.Yearly) => IntegrationEventContracts.MasterDataUpdated.Types.MeterReadingPeriodicity.MrpYearly,
                _ => IntegrationEventContracts.MasterDataUpdated.Types.MeterReadingPeriodicity.MrpNull,
            };
        }

        public static IntegrationEventContracts.MasterDataUpdated.Types.UnitType GetUnitType(
            this MasterDataWasUpdatedIntegrationEvent @event)
        {
            return @event.UnitType switch
            {
                nameof(MeasurementUnitType.Wh) => IntegrationEventContracts.MasterDataUpdated.Types.UnitType.UtWh,
                nameof(MeasurementUnitType.KWh) => IntegrationEventContracts.MasterDataUpdated.Types.UnitType.UtKwh,
                nameof(MeasurementUnitType.MWh) => IntegrationEventContracts.MasterDataUpdated.Types.UnitType.UtMwh,
                nameof(MeasurementUnitType.GWh) => IntegrationEventContracts.MasterDataUpdated.Types.UnitType.UtGwh,
                _ => IntegrationEventContracts.MasterDataUpdated.Types.UnitType.UtNull,
            };
        }
    }
}
