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

using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Queries;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints.Queries
{
    public class MeteringPointGsrnExistsQueryHandler : IRequestHandler<MeteringPointGsrnExistsQuery, bool>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MeteringPointGsrnExistsQueryHandler(
            IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> Handle(MeteringPointGsrnExistsQuery request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var sql = @"SELECT TOP 1 ([RecordId]) FROM [dbo].[MeteringPoints] WHERE GsrnNumber = @GsrnNumber";
            var result = await _connectionFactory
                .GetOpenConnection()
                .ExecuteScalarAsync<int?>(sql, new { request.GsrnNumber })
                .ConfigureAwait(false);

            return result.HasValue;
        }
    }
}
