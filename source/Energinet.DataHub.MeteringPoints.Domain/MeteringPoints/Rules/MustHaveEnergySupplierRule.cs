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

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.Rules
{
    public class MustHaveEnergySupplierRule : IBusinessRule
    {
        private readonly GsrnNumber _gsrnNumber;
        private readonly ConnectionDetails _connectDetails;
        private readonly EnergySupplierDetails? _energySupplierDetails;

        public MustHaveEnergySupplierRule(GsrnNumber gsrnNumber, ConnectionDetails connectDetails, EnergySupplierDetails? energySupplierDetails)
        {
            _gsrnNumber = gsrnNumber;
            _connectDetails = connectDetails;
            _energySupplierDetails = energySupplierDetails;
            IsBroken = energySupplierDetails is null || EffectiveDateIsOnOrAfterStartOfSupply() == false;
        }

        public bool IsBroken { get; }

        public ValidationError ValidationError => new MustHaveEnergySupplierRuleError(_gsrnNumber, _connectDetails.EffectiveDate);

        private bool EffectiveDateIsOnOrAfterStartOfSupply()
        {
            return _connectDetails.EffectiveDate >= _energySupplierDetails?.StartOfSupply;
        }
    }
}
