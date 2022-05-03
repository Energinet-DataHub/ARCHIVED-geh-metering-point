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

using System;
using System.Collections.Generic;
using Energinet.DataHub.MeteringPoints.Domain.Extensions;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Components;
using Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Errors;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MasterDataHandling.Rules
{
    public class DisconnectionTypeIsValidRule : IBusinessRule
    {
        private readonly string _disconnectionType;

        public DisconnectionTypeIsValidRule(string meteringPointType, string disconnectionType)
        {
            _disconnectionType = disconnectionType;
            IsBroken = !IsValidDisconnectionType(meteringPointType, disconnectionType);
        }

        public bool IsBroken { get; }

        public ValidationError ValidationError => new InvalidDisconnectionTypeValue(_disconnectionType);

        private static bool IsValidDisconnectionType(string meteringPointType, string disconnectionType)
        {
            if (meteringPointType.IsParent() && !string.IsNullOrEmpty(disconnectionType))
            {
                return new HashSet<string?>
                {
                    DisconnectionType.Manual.Name,
                    DisconnectionType.Remote.Name,
                }.Contains(disconnectionType);
            }
            else
            {
                return true;
            }
        }
    }
}
