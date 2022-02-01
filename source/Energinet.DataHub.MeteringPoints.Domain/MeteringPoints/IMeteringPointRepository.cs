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

using System.Threading.Tasks;

namespace Energinet.DataHub.MeteringPoints.Domain.MeteringPoints
{
    /// <summary>
    /// Repository for metering points
    /// </summary>
    public interface IMeteringPointRepository
    {
        /// <summary>
        /// Retrieves a metering point by GSRN number
        /// </summary>
        /// <param name="gsrnNumber"></param>
        /// <returns><see cref="MeteringPoint"/></returns>
        Task<MeteringPoint?> GetByGsrnNumberAsync(GsrnNumber gsrnNumber);

        /// <summary>
        /// Add new metering point to repository
        /// </summary>
        /// <param name="meteringPoint"></param>
        void Add(MeteringPoint meteringPoint);

        /// <summary>
        /// Returns the parent metering point of a child
        /// </summary>
        /// /// <param name="id"></param>
        /// <returns><see cref="MeteringPoint"/></returns>
        Task<MeteringPoint?> GetByIdAsync(MeteringPointId id);
    }
}
