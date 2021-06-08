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

using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Tests
{
    public static class SampleData
    {
        public static string GsrnNumber => "571234567891234568";

        public static string StreetName => "Test Road 1";

        public static string PostCode => "8000";

        public static string CityName => "Aarhus";

        public static string CountryCode => "DK";

        public static bool IsAddressWashable => true;

        public static string PhysicalStateName => PhysicalState.Connected.Name;

        public static string SubTypeName => MeteringPointSubType.Physical.Name;

        public static string TypeName => MeteringPointType.Consumption.Name;

        public static string PowerPlant => "571234567891234568";

        public static string LocationDescription => string.Empty;

        public static string ProductType => string.Empty;

        public static string ParentRelatedMeteringPoint => null;

        public static string UnitType => string.Empty;

        public static string MeterNumber => string.Empty;

        public static string MeterReadingOccurence => string.Empty;

        public static int MaximumCurrent => 0;

        public static int MaximumPower => 230;

        public static Instant? OccurenceDate => SystemClock.Instance.GetCurrentInstant();

        public static string SettlementMethod => string.Empty;

        public static string NetSettlementGroup => string.Empty;

        public static string MeteringGridArea => "822";

        public static string DisconnectionType => string.Empty;

        public static string ConnectionType => string.Empty;

        public static string AssetType => string.Empty;
    }
}
