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
using System.Globalization;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules
{
    public class ConnectionTypeRequirementRule : IBusinessRule
    {
        public ConnectionTypeRequirementRule(NetSettlementGroup netSettlementGroup, ConnectionType? connectionType)
        {
            if (netSettlementGroup == null!) throw new ArgumentNullException(nameof(netSettlementGroup));
            if (netSettlementGroup == NetSettlementGroup.Zero)
            {
                IsBroken = connectionType is not null;
                ValidationError = new ConnectionTypeIsNotAllowedRuleError();
                return;
            }

            if (netSettlementGroup != NetSettlementGroup.Zero && connectionType is null)
            {
                IsBroken = true;
                ValidationError = new ConnectionTypeIsRequiredRuleError();
                return;
            }

            if ((netSettlementGroup == NetSettlementGroup.Six || netSettlementGroup == NetSettlementGroup.Three) && connectionType! != ConnectionType.Installation)
            {
                IsBroken = true;
                ValidationError = new ConnectionTypeDoesNotMatchNetSettlementGroupRuleError(connectionType?.Name!, netSettlementGroup.Id.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool IsBroken { get; }

        public ValidationError ValidationError { get; } = new ConnectionTypeIsNotAllowedRuleError();
    }
}
