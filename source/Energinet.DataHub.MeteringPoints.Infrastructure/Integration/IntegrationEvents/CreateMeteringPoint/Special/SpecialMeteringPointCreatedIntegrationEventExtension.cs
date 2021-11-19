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

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Special
{
    internal static class SpecialMeteringPointCreatedIntegrationEventExtension
    {
        public static SpecialMeteringPointCreated.Types.MeterReadingPeriodicity GetMeterReadingPeriodicity(
            this SpecialMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.ReadingOccurrence switch
            {
                "Yearly" => SpecialMeteringPointCreated.Types.MeterReadingPeriodicity.MrpYearly,
                "Monthly" => SpecialMeteringPointCreated.Types.MeterReadingPeriodicity.MrpMonthly,
                "Hourly" => SpecialMeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly,
                "Quarterly" => SpecialMeteringPointCreated.Types.MeterReadingPeriodicity.MrpQuarterly,
                _ => throw new ArgumentException("Meter reading periodicity is not recognized."),
            };
        }

        public static SpecialMeteringPointCreated.Types.ProductType GetProductType(
            this SpecialMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.ProductType switch
            {
                "Tariff" => SpecialMeteringPointCreated.Types.ProductType.PtTariff,
                "FuelQuantity" => SpecialMeteringPointCreated.Types.ProductType.PtFuelquantity,
                "PowerActive" => SpecialMeteringPointCreated.Types.ProductType.PtPoweractive,
                "PowerReactive" => SpecialMeteringPointCreated.Types.ProductType.PtPowerreactive,
                "EnergyActive" => SpecialMeteringPointCreated.Types.ProductType.PtEnergyactive,
                "EnergyReactive" => SpecialMeteringPointCreated.Types.ProductType.PtEnergyreactive,
                _ => throw new ArgumentException("Product type is not recognized."),
            };
        }

        public static SpecialMeteringPointCreated.Types.ConnectionState GetConnectionState(
            this SpecialMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.ConnectionState switch
            {
                "New" => SpecialMeteringPointCreated.Types.ConnectionState.CsNew,
                _ => throw new ArgumentException("Meter reading periodicity is not recognized."),
            };
        }

        public static SpecialMeteringPointCreated.Types.UnitType GetUnitType(
            this SpecialMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.UnitType switch
            {
                "Wh" => SpecialMeteringPointCreated.Types.UnitType.UtWh,
                "KWh" => SpecialMeteringPointCreated.Types.UnitType.UtKwh,
                "MWh" => SpecialMeteringPointCreated.Types.UnitType.UtMwh,
                "GWh" => SpecialMeteringPointCreated.Types.UnitType.UtGwh,
                _ => throw new ArgumentException("Meter reading periodicity is not recognized."),
            };
        }
    }
}
