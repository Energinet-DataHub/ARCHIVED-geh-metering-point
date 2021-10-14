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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    public static class SampleData
    {
        public static string GsrnNumber => "571234567891234568";

        public static string TypeOfMeteringPoint => nameof(MeteringPointType.Consumption);

        public static string Transaction => Guid.NewGuid().ToString();

        public static string SubTypeOfMeteringPoint => MeteringMethod.Physical.Name;

        public static string SettlementMethod => Domain.MeteringPoints.Consumption.SettlementMethod.Flex.Name;

        public static string DisconnectionType => Domain.MeteringPoints.MarketMeteringPoints.DisconnectionType.Manual.Name;

        public static string ConnectionType => string.Empty;

        public static string PowerPlantGsrnNumber => "571234567891234568";

        public static string ReadingOccurrence => Domain.MeteringPoints.ReadingOccurrence.Hourly.Name;

        public static string AssetType => Domain.MeteringPoints.AssetType.GasTurbine.Name;

        public static string EffectiveDate => "2021-05-05T22:00:00Z";

        public static string MeasurementUnitType => Domain.MeteringPoints.MeasurementUnitType.KWh.Name;

        public static string MeteringGridArea => "870";

        public static string StreetName => "Test Road 1";

        public static string BuildingNumber => "145K";

        public static string PostCode => "8000";

        public static string CityName => "Aarhus";

        public static string CitySubDivisionName => "Aarhus";

        public static string MunicipalityCode => "124";

        public static string CountryCode => "DK";

        public static string StreetCode => "9999";

        public static bool IsActualAddress => true;

        public static string MeterNumber => "12345678910";

        public static string NetSettlementGroup => Domain.MeteringPoints.MarketMeteringPoints.NetSettlementGroup.Zero.Name;

        public static string FloorIdentification => string.Empty;

        public static string RoomIdentification => string.Empty;

        public static string ProductType => Domain.MeteringPoints.ProductType.EnergyActive.Name;

        public static string GeoInfoReference => Guid.NewGuid().ToString();

        public static string PhysicalState => Domain.MeteringPoints.PhysicalState.New.Name;

        public static string ScheduledMeterReadingDate => string.Empty;
    }
}
