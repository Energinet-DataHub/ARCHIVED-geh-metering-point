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
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using MediatR;

namespace Energinet.DataHub.MeteringPoints.Application.MarketParticipants.ActorsCreated;

public class ActorCreatedHandler : ICommandHandler<ActorCreated, Unit>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public ActorCreatedHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Unit> Handle(ActorCreated request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        var conn = _dbConnectionFactory.GetOpenConnection();
        var sql = @$"INSERT INTO [dbo].[Actor] ([Id], [IdentificationNumber], [IdentificationType]) VALUES (@Id, @IdentificationNumber, @Type)";
        await conn.ExecuteAsync(
            sql,
            new
            {
                Id = request.Id,
                IdentificationNumber = request.IdentificationNumber,
                Type = request.IdentificationType,
            }).ConfigureAwait(false);

        return await Task.FromResult(Unit.Value).ConfigureAwait(false);
    }
}
