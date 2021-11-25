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
    public enum MeteringPointTypeCode
    {
        D01 = 5, // VEProduction),
        D02 = 4, // Analysis),
        D20 = 6, // ExchangeReactiveEnergy),
        D99 = 7, // InternalUse),
        E17 = 0, // Consumption),
        E18 = 1, // Production),
        E20 = 2, // Exchange),
        D04 = 8, // SurplusProductionGroup),
        D05 = 9, // NetProduction),
        D06 = 10, // SupplyToGrid),
        D07 = 11, // ConsumptionFromGrid),
        D08 = 12, // WholesaleServices),
        D09 = 13, // OwnProduction),
        D10 = 14, // NetFromGrid),
        D11 = 15, // NetToGrid),
        D12 = 16, // TotalConsumption),
        D13 = 3, // GridLossCorrection),
        D14 = 17, // ElectricalHeating),
        D15 = 18, // NetConsumption),
        D17 = 19, // OtherConsumption),
        D18 = 20, // OtherProduction),
    }
}
