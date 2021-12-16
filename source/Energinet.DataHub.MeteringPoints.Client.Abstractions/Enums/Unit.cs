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
    public enum Unit
    {
        AMP = 0, //Ampere
        H87 = 9, //STK
        K3 = 1, //kVArh
        KWH = 2, //kWh
        KWT = 3, //kW
        MAW = 4, //MW
        MWH = 5, //MWh
        TNE = 6, //Tonne
        Z03 = 7, //MVAr
        Z14 = 8, //DanishTariffCode
    }
}
