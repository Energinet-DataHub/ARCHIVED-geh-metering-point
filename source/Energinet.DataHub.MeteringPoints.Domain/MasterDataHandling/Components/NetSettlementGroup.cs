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
    public class NetSettlementGroup : EnumerationType
    {
        public static readonly NetSettlementGroup Zero = new NetSettlementGroup(0, nameof(Zero));
        public static readonly NetSettlementGroup One = new NetSettlementGroup(1, nameof(One));
        public static readonly NetSettlementGroup Two = new NetSettlementGroup(2, nameof(Two));
        public static readonly NetSettlementGroup Three = new NetSettlementGroup(3, nameof(Three));
        public static readonly NetSettlementGroup Six = new NetSettlementGroup(6, nameof(Six));
        public static readonly NetSettlementGroup Ninetynine = new NetSettlementGroup(99, nameof(Ninetynine));

        private NetSettlementGroup(int id, string name)
            : base(id, name)
        {
        }
    }
}
