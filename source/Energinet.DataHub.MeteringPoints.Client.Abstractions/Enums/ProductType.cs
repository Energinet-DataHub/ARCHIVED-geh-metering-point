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
    public enum ProductType
    {
        Tariff = 0, //5790001330590
        FuelQuantity = 1, //5790001330606
        PowerActive = 2, //8716867000016
        PowerReactive = 3, //8716867000023
        EnergyActive = 4, //8716867000030
        EnergyReactive = 5, //8716867000047
    }
}
