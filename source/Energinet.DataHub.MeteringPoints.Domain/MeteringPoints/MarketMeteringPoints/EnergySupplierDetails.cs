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
using Energinet.DataHub.MeteringPoints.Domain.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints
{
    public class EnergySupplierDetails : ValueObject
    {
#pragma warning disable CS8618 // Ignore uninitialized properties
        protected EnergySupplierDetails() { }
#pragma warning restore

        private EnergySupplierDetails(
            Guid id,
            MeteringPointId meteringPointId,
            Instant startOfSupply,
            GlnNumber glnNumber)
        {
            MeteringPointId = meteringPointId;
            StartOfSupply = startOfSupply;
            GlnNumber = glnNumber;
            Id = id;
        }

        public Guid Id { get; set; } // TODO: TBA: value object

        public MeteringPointId MeteringPointId { get; set; }

        public Instant StartOfSupply { get; }

        public GlnNumber GlnNumber { get; }

        public static EnergySupplierDetails Create(MeteringPointId meteringPointId, Instant startOfSupply, GlnNumber gln)
        {
            return new EnergySupplierDetails(Guid.NewGuid(), meteringPointId, startOfSupply, gln);
        }
    }
}
