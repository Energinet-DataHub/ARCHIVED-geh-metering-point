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

using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    public class MeasurementUnitType : EnumerationType
    {
        public static readonly MeasurementUnitType Ampere = new MeasurementUnitType(0, nameof(Ampere));
        public static readonly MeasurementUnitType KVArh = new MeasurementUnitType(1, nameof(KVArh));
        public static readonly MeasurementUnitType KWh = new MeasurementUnitType(2, nameof(KWh));
        public static readonly MeasurementUnitType KW = new MeasurementUnitType(3, nameof(KW));
        public static readonly MeasurementUnitType MW = new MeasurementUnitType(4, nameof(MW));
        public static readonly MeasurementUnitType MWh = new MeasurementUnitType(5, nameof(MWh));
        public static readonly MeasurementUnitType Tonne = new MeasurementUnitType(6, nameof(Tonne));
        public static readonly MeasurementUnitType MVAr = new MeasurementUnitType(7, nameof(MVAr));
        public static readonly MeasurementUnitType DanishTariffCode = new MeasurementUnitType(8, nameof(DanishTariffCode));
        public static readonly MeasurementUnitType STK = new MeasurementUnitType(9, nameof(STK));

        private MeasurementUnitType(int id, string name)
            : base(id, name)
        {
        }
    }
}
