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

namespace Energinet.DataHub.MeteringPoints.Domain.GridAreas
{
    public class PriceAreaCode : EnumerationType
    {
        public static readonly PriceAreaCode DK1 = new(0, nameof(DK1), "DK1");
        public static readonly PriceAreaCode DK2 = new(0, nameof(DK2), "DK2");

        private PriceAreaCode(int id, string name, string code)
            : base(id, name)
        {
            Code = code;
        }

        public string Code { get; }
    }
}
