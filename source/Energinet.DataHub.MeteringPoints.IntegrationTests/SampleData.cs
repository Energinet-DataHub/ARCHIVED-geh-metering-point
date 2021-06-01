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

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    public static class SampleData
    {
        public static string GsrnNumber => "571234567891234568";

        public static string TypeOfMeteringPoint => "consumption";

        public static string Transaction => Guid.NewGuid().ToString();

        public static string SubTypeOfMeteringPoint => "physical";

        public static string SettlementMethod => "flex";

        public static string DisconnectionType => "manual";

        public static string ConnectionType => "installation";

        public static string PowerPlantGsrnNumber => "571234567891234568";

        public static string ReadingOccurrence => "hourly";

        public static string AssetType => "gasTurbine";

        public static string Occurrence => "2021-05-05T10:10:10Z";

        public static string MeasurementUnitType => "kWh";

        public static string MeteringGridArea => "990";

        public static string StreetName => "Test Road 1";

        public static string PostCode => "8000";

        public static string CityName => "Aarhus";

        public static string CountryCode => "DK";

        public static string MeterNumber => "12345678910";
    }
}
