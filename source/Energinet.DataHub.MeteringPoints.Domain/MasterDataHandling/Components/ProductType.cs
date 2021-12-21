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
    public class ProductType : EnumerationType
    {
        public static readonly ProductType Tariff = new ProductType(0, nameof(Tariff), "5790001330590");
        public static readonly ProductType FuelQuantity = new ProductType(1, nameof(FuelQuantity), "5790001330606");
        public static readonly ProductType PowerActive = new ProductType(2, nameof(PowerActive), "8716867000016");
        public static readonly ProductType PowerReactive = new ProductType(3, nameof(PowerReactive), "8716867000023");
        public static readonly ProductType EnergyActive = new ProductType(4, nameof(EnergyActive), "8716867000030");
        public static readonly ProductType EnergyReactive = new ProductType(5, nameof(EnergyReactive), "8716867000047");

        private ProductType(int id, string name, string code)
            : base(id, name)
        {
            Code = code;
        }

        public string Code { get; }
    }
}
