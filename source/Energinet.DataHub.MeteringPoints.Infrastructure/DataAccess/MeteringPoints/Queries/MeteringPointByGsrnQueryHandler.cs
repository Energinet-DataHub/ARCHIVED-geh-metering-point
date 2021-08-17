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
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints.Queries
{
    public class MeteringPointByGsrnQueryHandler : IRequestHandler<MeteringPointByGsrnQuery, ConsumptionMeteringPoint>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MeteringPointByGsrnQueryHandler(
            IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Task<ConsumptionMeteringPoint> Handle(MeteringPointByGsrnQuery request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var sql = @"SELECT * FROM [dbo].[MeteringPoint] WHERE Id = @GsrnNumber";
            return _connectionFactory.GetOpenConnection().QuerySingleOrDefaultAsync<ConsumptionMeteringPoint>(sql, new { request.GsrnNumber });
        }
    }
}
