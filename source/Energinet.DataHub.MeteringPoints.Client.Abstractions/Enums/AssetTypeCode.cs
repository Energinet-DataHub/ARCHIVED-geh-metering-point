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

namespace Energinet.DataHub.MeteringPoints.Client.Abstractions.Enums
{
    public enum AssetTypeCode
    {
        D01 = 0, // SteamTurbineWithBackPressureMode
        D02 = 1, // GasTurbine
        D03 = 2, // CombinedCycle
        D04 = 3, // CombustionEngineGas
        D05 = 4, // SteamTurbineWithCondensation
        D06 = 5, // Boiler
        D07 = 6, // StirlingEngine
        D10 = 7, // FuelCells
        D11 = 8, // PhotovoltaicCells
        D12 = 9, // WindTurbines
        D13 = 10, // HydroelectricPower
        D14 = 11, // WavePower
        D17 = 12, // DispatchableWindTurbines
        D19 = 13, // DieselCombustionEngine
        D20 = 14, // BioCombustionEngine
        D99 = 100, // UnknownTechnology
    }
}
