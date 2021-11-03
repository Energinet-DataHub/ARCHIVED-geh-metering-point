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

using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;

namespace Energinet.DataHub.MeteringPoints.Client.Abstractions
{
    /// <summary>
    /// Metering point client
    /// </summary>
    public interface IMeteringPointClient
    {
        /// <summary>
        /// Get a single metering point by GSRN number
        /// </summary>
        /// <param name="gsrn">GSRN number to identify a Metering Point.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>A Metering Point DTO if found. If not found null will be returned.</returns>
        public Task<MeteringPointDto?> GetMeteringPointByGsrnAsync(string gsrn, CancellationToken cancellationToken = default);
    }
}
