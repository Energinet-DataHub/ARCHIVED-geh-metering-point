﻿// Copyright 2020 Energinet DataHub A/S
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
        public static readonly MeteringPointType Consumption = new MeteringPointType(0, nameof(Consumption), 1, true);
        public static readonly MeteringPointType Production = new MeteringPointType(1, nameof(Production), 1, true);
        public static readonly MeteringPointType Exchange = new MeteringPointType(2, nameof(Exchange), 2);
        public static readonly MeteringPointType GridLossCorrection = new MeteringPointType(3, nameof(GridLossCorrection), 3);
        public static readonly MeteringPointType Analysis = new MeteringPointType(4, nameof(Analysis), 4);
        public static readonly MeteringPointType VEProduction = new MeteringPointType(5, nameof(VEProduction), 4);
        public static readonly MeteringPointType ExchangeReactiveEnergy = new MeteringPointType(6, nameof(ExchangeReactiveEnergy), 5);
        public static readonly MeteringPointType InternalUse = new MeteringPointType(7, nameof(InternalUse), 4);
        public static readonly MeteringPointType SurplusProductionGroup = new MeteringPointType(8, nameof(SurplusProductionGroup), 3);
        public static readonly MeteringPointType NetProduction = new MeteringPointType(9, nameof(NetProduction), 3);
        public static readonly MeteringPointType SupplyToGrid = new MeteringPointType(10, nameof(SupplyToGrid), 3);
        public static readonly MeteringPointType ConsumptionFromGrid = new MeteringPointType(11, nameof(ConsumptionFromGrid), 3);
        public static readonly MeteringPointType WholesaleServices = new MeteringPointType(12, nameof(WholesaleServices), 3);
        public static readonly MeteringPointType OwnProduction = new MeteringPointType(13, nameof(OwnProduction), 3);
        public static readonly MeteringPointType NetFromGrid = new MeteringPointType(14, nameof(NetFromGrid), 3);
        public static readonly MeteringPointType NetToGrid = new MeteringPointType(15, nameof(NetToGrid), 3);
        public static readonly MeteringPointType TotalConsumption = new MeteringPointType(16, nameof(TotalConsumption), 3);
        public static readonly MeteringPointType ElectricalHeating = new MeteringPointType(17, nameof(ElectricalHeating), 3);
        public static readonly MeteringPointType NetConsumption = new MeteringPointType(18, nameof(NetConsumption), 3);
        public static readonly MeteringPointType OtherConsumption = new MeteringPointType(19, nameof(OtherConsumption), 3);
        public static readonly MeteringPointType OtherProduction = new MeteringPointType(20, nameof(OtherProduction), 3);

        private MeteringPointType(int id, string name, int meteringPointGroup, bool isAccountingPoint = false)
            : base(id, name)
        {
            MeteringPointGroup = meteringPointGroup;
            IsAccountingPoint = isAccountingPoint;
        }

        public int MeteringPointGroup { get; }

        public bool IsAccountingPoint { get; }
    }
}
