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
using Energinet.DataHub.MeteringPoints.IntegrationEvents.CreateMeteringPoint;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.IntegrationEvents.CreateMeteringPoint.Exchange
{
    internal static class ExchangeMeteringPointCreatedIntegrationEventExtension
    {
        public static ExchangeMeteringPointCreated.Types.ProductType GetProductType(
            this ExchangeMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.ProductType switch
            {
                "Tariff" => ExchangeMeteringPointCreated.Types.ProductType.PtTariff,
                "FuelQuantity" => ExchangeMeteringPointCreated.Types.ProductType.PtFuelquantity,
                "PowerActive" => ExchangeMeteringPointCreated.Types.ProductType.PtPoweractive,
                "PowerReactive" => ExchangeMeteringPointCreated.Types.ProductType.PtPowerreactive,
                "EnergyActive" => ExchangeMeteringPointCreated.Types.ProductType.PtEnergyactive,
                "EnergyReactive" => ExchangeMeteringPointCreated.Types.ProductType.PtEnergyreactive,
                _ => throw new ArgumentException("Product type is not recognized."),
            };
        }

        public static ExchangeMeteringPointCreated.Types.MeteringMethod GetMeteringMethod(
            this ExchangeMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.MeteringMethod switch
            {
                "Physical" => ExchangeMeteringPointCreated.Types.MeteringMethod.MmPhysical,
                "Virtual" => ExchangeMeteringPointCreated.Types.MeteringMethod.MmVirtual,
                "Calculated" => ExchangeMeteringPointCreated.Types.MeteringMethod.MmCalculated,
                _ => throw new ArgumentException("Metering method is not recognized."),
            };
        }

        public static ExchangeMeteringPointCreated.Types.MeterReadingPeriodicity GetMeterReadingPeriodicity(
            this ExchangeMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.MeterReadingPeriodicity switch
            {
                "Hourly" => ExchangeMeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly,
                "Quarterly" => ExchangeMeteringPointCreated.Types.MeterReadingPeriodicity.MrpQuarterly,
                _ => throw new ArgumentException("Meter reading periodicity is not recognized."),
            };
        }

        public static ExchangeMeteringPointCreated.Types.ConnectionState GetConnectionState(
            this ExchangeMeteringPointCreatedIntegrationEvent @event)
        {
            return @event.ConnectionState switch
            {
                "New" => ExchangeMeteringPointCreated.Types.ConnectionState.CsNew,
                _ => throw new ArgumentException("Meter reading periodicity is not recognized."),
            };
        }
    }
}
