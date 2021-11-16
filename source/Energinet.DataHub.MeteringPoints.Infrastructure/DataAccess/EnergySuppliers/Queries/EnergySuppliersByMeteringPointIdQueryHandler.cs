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
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.MeteringPoints.Application.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Application.EnergySuppliers.Queries;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.EnergySuppliers.Queries
{
    public class EnergySuppliersByMeteringPointIdQueryHandler : IRequestHandler<EnergySuppliersByMeteringPointIdQuery, IEnumerable<EnergySupplierDto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public EnergySuppliersByMeteringPointIdQueryHandler(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<EnergySupplierDto>> Handle(EnergySuppliersByMeteringPointIdQuery request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var sql = @"SELECT [Id]
                              ,[StartOfSupplyDate]
                              ,[GlnNumber]
                          FROM [dbo].[EnergySuppliers]
                         WHERE [MarketMeteringPointId] = @MeteringPointId";

            var result = await _connectionFactory
                .GetOpenConnection()
                .QueryAsync<EnergySupplierDto>(sql, new { request.MeteringPointId })
                .ConfigureAwait(false);

            return result;
        }
    }
}
