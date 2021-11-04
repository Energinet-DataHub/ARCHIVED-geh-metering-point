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

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Production
{
    internal static class ProductionMeteringPointCreatedIntegrationEventExtension
    {
        public static ProductionMeteringPointCreated.Types.NetSettlementGroup GetNetSettlementGroup(
            this ProductionMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.NetSettlementGroup switch
            {
                "Zero" => ProductionMeteringPointCreated.Types.NetSettlementGroup.NsgZero,
                "One" => ProductionMeteringPointCreated.Types.NetSettlementGroup.NsgOne,
                "Two" => ProductionMeteringPointCreated.Types.NetSettlementGroup.NsgTwo,
                "Three" => ProductionMeteringPointCreated.Types.NetSettlementGroup.NsgThree,
                "Six" => ProductionMeteringPointCreated.Types.NetSettlementGroup.NsgSix,
                "NinetyNine" => ProductionMeteringPointCreated.Types.NetSettlementGroup.NsgNinetynine,
                _ => throw new ArgumentException("Net settlement group is not recognized."),
            };
        }

        public static ProductionMeteringPointCreated.Types.ProductType GetProductType(
            this ProductionMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.ProductType switch
            {
                "Tariff" => ProductionMeteringPointCreated.Types.ProductType.PtTariff,
                "FuelQuantity" => ProductionMeteringPointCreated.Types.ProductType.PtFuelquantity,
                "PowerActive" => ProductionMeteringPointCreated.Types.ProductType.PtPoweractive,
                "PowerReactive" => ProductionMeteringPointCreated.Types.ProductType.PtPowerreactive,
                "EnergyActive" => ProductionMeteringPointCreated.Types.ProductType.PtEnergyactive,
                "EnergyReactive" => ProductionMeteringPointCreated.Types.ProductType.PtEnergyreactive,
                _ => throw new ArgumentException("Product type is not recognized."),
            };
        }

        public static ProductionMeteringPointCreated.Types.MeteringMethod GetMeteringMethod(
            this ProductionMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.MeteringMethod switch
            {
                "Physical" => ProductionMeteringPointCreated.Types.MeteringMethod.MmPhysical,
                "Virtual" => ProductionMeteringPointCreated.Types.MeteringMethod.MmVirtual,
                "Calculated" => ProductionMeteringPointCreated.Types.MeteringMethod.MmCalculated,
                _ => throw new ArgumentException("Metering method is not recognized."),
            };
        }

        public static ProductionMeteringPointCreated.Types.MeterReadingPeriodicity GetMeterReadingPeriodicity(
            this ProductionMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.MeterReadingPeriodicity switch
            {
                "Hourly" => ProductionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly,
                "Quarterly" => ProductionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpQuarterly,
                _ => throw new ArgumentException("Meter reading periodicity is not recognized."),
            };
        }

        public static ProductionMeteringPointCreated.Types.ConnectionState GetConnectionState(
            this ProductionMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.ConnectionState switch
            {
                "New" => ProductionMeteringPointCreated.Types.ConnectionState.CsNew,
                _ => throw new ArgumentException("Meter reading periodicity is not recognized."),
            };
        }

        public static ProductionMeteringPointCreated.Types.UnitType GetUnitType(
            this ProductionMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.UnitType switch
            {
                "Wh" => ProductionMeteringPointCreated.Types.UnitType.UtWh,
                "KWh" => ProductionMeteringPointCreated.Types.UnitType.UtKwh,
                "MWh" => ProductionMeteringPointCreated.Types.UnitType.UtMwh,
                "GWh" => ProductionMeteringPointCreated.Types.UnitType.UtGwh,
                _ => throw new ArgumentException("Meter reading periodicity is not recognized."),
            };
        }
    }
}
