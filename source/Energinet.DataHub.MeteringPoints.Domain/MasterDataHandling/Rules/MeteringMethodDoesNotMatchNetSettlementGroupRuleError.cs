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

using System;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components.MeteringDetails;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules
{
    public class MeteringMethodDoesNotMatchNetSettlementGroupRuleError : ValidationError
    {
        public MeteringMethodDoesNotMatchNetSettlementGroupRuleError(MeteringMethod meteringMethod)
        {
            if (meteringMethod is null) throw new ArgumentNullException(nameof(meteringMethod));
            Code = "D37";
            Message = $"Metering method {meteringMethod.Name} not allowed: the metering method for this type of metering point must be Virtual (D02) or Calculated (D03) if net settlement group is not 0 or 99.";
        }
    }
}
