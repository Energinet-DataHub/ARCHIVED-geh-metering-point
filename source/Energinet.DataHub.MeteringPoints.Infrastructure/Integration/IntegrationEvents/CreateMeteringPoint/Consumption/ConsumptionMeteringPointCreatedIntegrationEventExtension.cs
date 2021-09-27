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

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Consumption
{
    internal static class ConsumptionMeteringPointCreatedIntegrationEventExtension
    {
        public static ConsumptionMeteringPointCreated.Types.SettlementMethod GetSettlementMethod(
            this ConsumptionMeteringPointCreatedIntegrationEvent ev)
        {
            return ev.SettlementMethod switch
            {
                "Flex" => ConsumptionMeteringPointCreated.Types.SettlementMethod.Flex,
                "NonProfiled" => ConsumptionMeteringPointCreated.Types.SettlementMethod.Nonprofiled,
                "Profiled" => ConsumptionMeteringPointCreated.Types.SettlementMethod.Profiled,
                _ => throw new ArgumentException("Settlement method is not recognized."),
            };
        }

        public static ConsumptionMeteringPointCreated.Types.NetSettlementGroup GetNetSettlementGroup(
            this ConsumptionMeteringPointCreatedIntegrationEvent ev)
        {
            return ev.NetSettlementGroup switch
            {
                "Zero" => ConsumptionMeteringPointCreated.Types.NetSettlementGroup.Zero,
                "One" => ConsumptionMeteringPointCreated.Types.NetSettlementGroup.One,
                "Two" => ConsumptionMeteringPointCreated.Types.NetSettlementGroup.Two,
                "Three" => ConsumptionMeteringPointCreated.Types.NetSettlementGroup.Three,
                "Six" => ConsumptionMeteringPointCreated.Types.NetSettlementGroup.Six,
                "NinetyNine" => ConsumptionMeteringPointCreated.Types.NetSettlementGroup.Ninetynine,
                _ => throw new ArgumentException("Net settlement group is not recognized."),
            };
        }

        public static ConsumptionMeteringPointCreated.Types.ProductType GetProductType(
            this ConsumptionMeteringPointCreatedIntegrationEvent ev)
        {
            return ev.ProductType switch
            {
                "Tariff" => ConsumptionMeteringPointCreated.Types.ProductType.Tariff,
                "FuelQuantity" => ConsumptionMeteringPointCreated.Types.ProductType.Fuelquantity,
                "PowerActive" => ConsumptionMeteringPointCreated.Types.ProductType.Poweractive,
                "PowerReactive" => ConsumptionMeteringPointCreated.Types.ProductType.Powerreactive,
                "EnergyActive" => ConsumptionMeteringPointCreated.Types.ProductType.Energyactive,
                "EnergyReactive" => ConsumptionMeteringPointCreated.Types.ProductType.Energyreactive,
                _ => throw new ArgumentException("Product type is not recognized."),
            };
        }

        public static ConsumptionMeteringPointCreated.Types.MeteringMethod GetMeteringMethod(
            this ConsumptionMeteringPointCreatedIntegrationEvent ev)
        {
            return ev.MeteringMethod switch
            {
                "Physical" => ConsumptionMeteringPointCreated.Types.MeteringMethod.Physical,
                "Virtual" => ConsumptionMeteringPointCreated.Types.MeteringMethod.Virtual,
                "Calculated" => ConsumptionMeteringPointCreated.Types.MeteringMethod.Calculated,
                _ => throw new ArgumentException("Metering method is not recognized."),
            };
        }

        public static ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity GetMeterReadingPeriodicity(
            this ConsumptionMeteringPointCreatedIntegrationEvent ev)
        {
            return ev.MeterReadingPeriodicity switch
            {
                "Yearly" => ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.Yearly,
                "Monthly" => ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.Monthly,
                "Hourly" => ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.Hourly,
                "Quarterly" => ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.Quarterly,
                _ => throw new ArgumentException("Meter reading periodicity is not recognized."),
            };
        }
    }
}
