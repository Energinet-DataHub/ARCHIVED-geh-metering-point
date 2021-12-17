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
                _ => throw new ArgumentException("Product type is not recognized."),
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
    }
}
