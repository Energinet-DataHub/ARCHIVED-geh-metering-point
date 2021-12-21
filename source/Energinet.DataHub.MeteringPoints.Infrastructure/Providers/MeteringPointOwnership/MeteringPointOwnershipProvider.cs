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
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.MeteringPoints.Application.Providers.MeteringPointOwnership;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Providers.MeteringPointOwnership
{
    public class MeteringPointOwnershipProvider : IMeteringPointOwnershipProvider
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public MeteringPointOwnershipProvider(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<Owner> GetOwnerAsync(MeteringPoint meteringPoint)
        {
            if (meteringPoint == null) throw new ArgumentNullException(nameof(meteringPoint));
            var gridOperatorId = await _dbConnectionFactory.GetOpenConnection().QueryFirstOrDefaultAsync<Guid>(
                    $"SELECT ga.ActorId FROM GridAreas ga " +
                    $"JOIN GridAreaLinks gl on ga.Id = gl.GridAreaId " +
                    $"JOIN MeteringPoints mp on mp.MeteringGridArea = gl.Id " +
                    $"WHERE mp.GsrnNumber = @GsrnNumber",
                    new { GsrnNumber = meteringPoint.GsrnNumber.Value })
                .ConfigureAwait(false);

            if (gridOperatorId == Guid.Empty)
            {
                throw new MeteringPointOwnershipProviderException($"Could not determine owner of metering point with GSRN-number '{meteringPoint.GsrnNumber.Value}'.");
            }

            return new Owner(gridOperatorId);
        }
    }
}
