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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints.MarketMeteringPoints;
using Energinet.DataHub.MeteringPoints.Domain.SeedWork;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Domain.EnergySuppliers
{
    public class EnergySupplier : Entity
    {
#pragma warning disable CS8618 // Ignore uninitialized properties
        protected EnergySupplier() { }
#pragma warning restore

        private EnergySupplier(
            Guid id,
            MeteringPointId marketMeteringPointId,
            Instant startOfSupply,
            GlnNumber glnNumber)
        {
            Id = id;
            MarketMeteringPointId = marketMeteringPointId;
            StartOfSupply = startOfSupply;
            GlnNumber = glnNumber;
        }

        public Guid Id { get; }

        public MeteringPointId MarketMeteringPointId { get; }

        public Instant StartOfSupply { get; }

        public GlnNumber GlnNumber { get; }

        public static EnergySupplier Create(MeteringPointId meteringPointId, Instant startOfSupply, GlnNumber gln)
        {
            return new EnergySupplier(Guid.NewGuid(), meteringPointId, startOfSupply, gln);
        }
    }
}
