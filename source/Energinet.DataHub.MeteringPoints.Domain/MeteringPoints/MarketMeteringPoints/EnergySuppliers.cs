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

using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.MeteringPoints.Domain.Extensions;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints
{
    internal class EnergySuppliers
    {
        private readonly LinkedList<EnergySupplierDetails> _energySupplierDetails;

        private EnergySuppliers(IEnumerable<EnergySupplierDetails> energySupplierDetails)
        {
            var orderedEnergySuppliers = energySupplierDetails.OrderBy(details => details.StartOfSupply);
            _energySupplierDetails = new LinkedList<EnergySupplierDetails>(orderedEnergySuppliers);
        }

        internal static EnergySuppliers Create(IEnumerable<EnergySupplierDetails> energySupplierDetails)
        {
            return new EnergySuppliers(energySupplierDetails);
        }

        internal EnergySupplierDetails? GetCurrent(Instant when)
        {
            var currentEnergySupplier = _energySupplierDetails
                .LastOrDefault(e => e.StartOfSupply.ToDate() <= when.ToDate());

            return currentEnergySupplier;
        }

        internal IEnumerable<EnergySupplierDetails> GetFutureEnergySuppliers(Instant when)
        {
            var futureEnergySuppliers = _energySupplierDetails
                .Where(e => e.StartOfSupply.ToDate() > when.ToDate())
                .ToList();

            return futureEnergySuppliers;
        }

        internal EnergySupplierDetails? GetFutureEnergySupplier(Instant when)
        {
            var currentEnergySupplier = _energySupplierDetails
                .FirstOrDefault(e => e.StartOfSupply.ToDate() >= when.ToDate());

            return currentEnergySupplier;
        }

        internal EnergySupplierDetails? GetNext(EnergySupplierDetails current)
        {
            var nextEnergySupplier = _energySupplierDetails.Find(current)?.Next?.Value;
            return nextEnergySupplier;
        }

        internal void Add(EnergySupplierDetails energySupplierDetails)
        {
            var currentForSpecifiedStartOfSupply = GetCurrent(energySupplierDetails.StartOfSupply);
            if (currentForSpecifiedStartOfSupply == null)
            {
                _energySupplierDetails.AddFirst(energySupplierDetails);
            }
            else
            {
                var currentNode = _energySupplierDetails.Find(currentForSpecifiedStartOfSupply);
                _energySupplierDetails.AddAfter(
                    currentNode!,
                    new LinkedListNode<EnergySupplierDetails>(energySupplierDetails));
            }
        }

        internal bool CanAdd(EnergySupplierDetails energySupplierDetails)
        {
            var currentForSpecifiedStartOfSupply = GetCurrent(energySupplierDetails.StartOfSupply);
            if (currentForSpecifiedStartOfSupply?.StartOfSupply.ToDate() == energySupplierDetails.StartOfSupply.ToDate())
            {
                return false;
            }

            return true;
        }
    }
}
