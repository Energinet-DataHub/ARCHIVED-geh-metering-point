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

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components
{
    public class AssetType : EnumerationType
    {
        public static readonly AssetType SteamTurbineWithBackPressureMode = new AssetType(0, nameof(SteamTurbineWithBackPressureMode));
        public static readonly AssetType GasTurbine = new AssetType(1, nameof(GasTurbine));
        public static readonly AssetType CombinedCycle = new AssetType(2, nameof(CombinedCycle));
        public static readonly AssetType CombustionEngineGas = new AssetType(3, nameof(CombustionEngineGas));
        public static readonly AssetType SteamTurbineWithCondensation = new AssetType(4, nameof(SteamTurbineWithCondensation));
        public static readonly AssetType Boiler = new AssetType(5, nameof(Boiler));
        public static readonly AssetType StirlingEngine = new AssetType(6, nameof(StirlingEngine));
        public static readonly AssetType FuelCells = new AssetType(7, nameof(FuelCells));
        public static readonly AssetType PhotovoltaicCells = new AssetType(8, nameof(PhotovoltaicCells));
        public static readonly AssetType WindTurbines = new AssetType(9, nameof(WindTurbines));
        public static readonly AssetType HydroelectricPower = new AssetType(10, nameof(HydroelectricPower));
        public static readonly AssetType WavePower = new AssetType(11, nameof(WavePower));
        public static readonly AssetType DispatchableWindTurbines = new AssetType(12, nameof(DispatchableWindTurbines));
        public static readonly AssetType DieselCombustionEngine = new AssetType(13, nameof(DieselCombustionEngine));
        public static readonly AssetType BioCombustionEngine = new AssetType(14, nameof(BioCombustionEngine));
        public static readonly AssetType NoTechnology = new AssetType(99, nameof(NoTechnology));
        public static readonly AssetType UnknownTechnology = new AssetType(100, nameof(UnknownTechnology));

        private AssetType(int id, string name)
            : base(id, name)
        {
        }
    }
}
