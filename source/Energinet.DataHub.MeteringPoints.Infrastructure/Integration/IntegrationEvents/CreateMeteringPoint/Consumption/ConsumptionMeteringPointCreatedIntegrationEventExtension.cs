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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Consumption;
using ConsumptionMeteringPointCreated = Energinet.DataHub.MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Consumption
{
    internal static class ConsumptionMeteringPointCreatedIntegrationEventExtension
    {
        public static ConsumptionMeteringPointCreated.Types.SettlementMethod GetSettlementMethod(
            this ConsumptionMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.SettlementMethod switch
            {
                "Flex" => ConsumptionMeteringPointCreated.Types.SettlementMethod.SmFlex,
                "NonProfiled" => ConsumptionMeteringPointCreated.Types.SettlementMethod.SmNonprofiled,
                "Profiled" => ConsumptionMeteringPointCreated.Types.SettlementMethod.SmProfiled,
                _ => throw new ArgumentException("Settlement method is not recognized."),
            };
        }

        public static ConsumptionMeteringPointCreated.Types.NetSettlementGroup GetNetSettlementGroup(
            this ConsumptionMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.NetSettlementGroup switch
            {
                "Zero" => ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgZero,
                "One" => ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgOne,
                "Two" => ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgTwo,
                "Three" => ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgThree,
                "Six" => ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgSix,
                "NinetyNine" => ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgNinetynine,
                _ => throw new ArgumentException("Net settlement group is not recognized."),
            };
        }

        public static ConsumptionMeteringPointCreated.Types.ProductType GetProductType(
            this ConsumptionMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.ProductType switch
            {
                "Tariff" => ConsumptionMeteringPointCreated.Types.ProductType.PtTariff,
                "FuelQuantity" => ConsumptionMeteringPointCreated.Types.ProductType.PtFuelquantity,
                "PowerActive" => ConsumptionMeteringPointCreated.Types.ProductType.PtPoweractive,
                "PowerReactive" => ConsumptionMeteringPointCreated.Types.ProductType.PtPowerreactive,
                "EnergyActive" => ConsumptionMeteringPointCreated.Types.ProductType.PtEnergyactive,
                "EnergyReactive" => ConsumptionMeteringPointCreated.Types.ProductType.PtEnergyreactive,
                _ => throw new ArgumentException("Product type is not recognized."),
            };
        }

        public static ConsumptionMeteringPointCreated.Types.MeteringMethod GetMeteringMethod(
            this ConsumptionMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.MeteringMethod switch
            {
                "Physical" => ConsumptionMeteringPointCreated.Types.MeteringMethod.MmPhysical,
                "Virtual" => ConsumptionMeteringPointCreated.Types.MeteringMethod.MmVirtual,
                "Calculated" => ConsumptionMeteringPointCreated.Types.MeteringMethod.MmCalculated,
                _ => throw new ArgumentException("Metering method is not recognized."),
            };
        }

        public static ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity GetMeterReadingPeriodicity(
            this ConsumptionMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.MeterReadingPeriodicity switch
            {
                "Yearly" => ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpYearly,
                "Monthly" => ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpMonthly,
                "Hourly" => ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly,
                "Quarterly" => ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpQuarterly,
                _ => throw new ArgumentException("Meter reading periodicity is not recognized."),
            };
        }
    }
}
