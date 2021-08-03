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
    public class MeteringPointType : EnumerationType
    {
        public static readonly MeteringPointType Consumption = new MeteringPointType(0, nameof(Consumption));
        public static readonly MeteringPointType Production = new MeteringPointType(1, nameof(Production));
        public static readonly MeteringPointType Exchange = new MeteringPointType(2, nameof(Exchange));
        public static readonly MeteringPointType GridLossCorrection = new MeteringPointType(3, nameof(GridLossCorrection));
        public static readonly MeteringPointType Analysis = new MeteringPointType(4, nameof(Analysis));
        public static readonly MeteringPointType VEProduction = new MeteringPointType(5, nameof(VEProduction));
        public static readonly MeteringPointType ExchangeReactiveEnergy = new MeteringPointType(6, nameof(ExchangeReactiveEnergy));
        public static readonly MeteringPointType InternalUse = new MeteringPointType(7, nameof(InternalUse));

        private MeteringPointType(int id, string name)
            : base(id, name)
        {
        }
    }
}
