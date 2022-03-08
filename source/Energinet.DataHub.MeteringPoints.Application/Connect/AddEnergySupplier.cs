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
using System.Text.Json.Serialization;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using NodaTime;

namespace Energinet.DataHub.MeteringPoints.Application.Connect
{
    public class AddEnergySupplier : InternalCommand
    {
        [JsonConstructor]
        public AddEnergySupplier(
            Guid id,
            string meteringPointId,
            Instant startOfSupply,
            string energySupplierGlnNumber)
        : base(id)
        {
            MeteringPointId = meteringPointId;
            StartOfSupply = startOfSupply;
            EnergySupplierGlnNumber = energySupplierGlnNumber;
        }

        public AddEnergySupplier(
                string meteringPointId,
                Instant startOfSupply,
                string energySupplierGlnNumber)
        {
            MeteringPointId = meteringPointId;
            StartOfSupply = startOfSupply;
            EnergySupplierGlnNumber = energySupplierGlnNumber;
        }

        public string MeteringPointId { get; }

        public Instant StartOfSupply { get; }

        public string EnergySupplierGlnNumber { get; }
    }
}
