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
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.IntegrationTests.Tooling;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    public static class SampleData
    {
        public static string Administrator => "90f6f4e5-8073-4842-b33e-c59e9f4a8c3f";

        public static string GsrnNumber => "571234567891234568";

        public static string TypeOfMeteringPoint => nameof(MeteringPointType.Consumption);

        public static string Transaction => "175C1079-4E53-47CB-BE27-02E9BB674487";

        public static string SubTypeOfMeteringPoint => MeteringMethod.Physical.Name;

        public static string SettlementMethod => Domain.MasterDataHandling.Components.SettlementMethod.Flex.Name;

        public static string DisconnectionType => Domain.MasterDataHandling.Components.DisconnectionType.Manual.Name;

        public static string ConnectionType => string.Empty;

        public static string PowerPlantGsrnNumber => "571234567891234568";

        public static string ReadingOccurrence => Domain.MasterDataHandling.Components.ReadingOccurrence.Hourly.Name;

        public static string AssetType => Domain.MasterDataHandling.Components.AssetType.GasTurbine.Name;

        public static string EffectiveDate => GetEffectiveDate();

        public static string MeasurementUnitType => Domain.MasterDataHandling.Components.MeasurementUnitType.KWh.Name;

        public static string MeteringGridArea => "870";

        public static Guid GridOperatorIdOfGrid870 => Guid.Parse("90f6f4e5-8073-4842-b33e-c59e9f4a8c3f");

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

        public static string NetSettlementGroup => Domain.MasterDataHandling.Components.NetSettlementGroup.Zero.Name;

        public static string FloorIdentification => string.Empty;

        public static string RoomIdentification => string.Empty;

        public static string ProductType => Domain.MasterDataHandling.Components.ProductType.EnergyActive.Name;

        public static string GeoInfoReference => Guid.NewGuid().ToString();

        public static string PhysicalState => Domain.MeteringPoints.PhysicalState.New.Name;

        public static string ScheduledMeterReadingDate => string.Empty;

        public static string GlnNumber => "3963865549824";

        public static string ParentGsrnNumber => "570851247381952311";

        private static string GetEffectiveDate()
        {
            var effectiveDate = Instant.FromUtc(
                2021,
                5,
                5,
                22,
                0,
                0);

            return TestHelpers.DaylightSavingsString(effectiveDate.ToDateTimeUtc());
        }
    }
}
