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
using Energinet.DataHub.MeteringPoints.Domain.Addresses;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules
{
    public class StreetNameIsRequiredRule : IBusinessRule
    {
        private readonly GsrnNumber _meteringpointGsrn;

        public StreetNameIsRequiredRule(GsrnNumber meteringpointGSRN, Address address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            _meteringpointGsrn = meteringpointGSRN;
            IsBroken = string.IsNullOrWhiteSpace(address.StreetName);
        }

        public StreetNameIsRequiredRule(GsrnNumber gsrnNumber, string? streetName)
        {
            _meteringpointGsrn = gsrnNumber;
            IsBroken = streetName?.Length == 0;
        }

        public bool IsBroken { get; }

        public ValidationError ValidationError => new StreetNameIsRequiredRuleError(_meteringpointGsrn);
    }
}
