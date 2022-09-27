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
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Application.Providers;
using Energinet.DataHub.MeteringPoints.Domain.Actors;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Providers
{
    public class ActorLookup : IActorLookup
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public ActorLookup(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public Task<Guid> GetIdByActorNumberAsync(string actorNumber)
        {
            return _dbConnectionFactory
                .GetOpenConnection()
                .ExecuteScalarAsync<Guid>(
                    "SELECT Id FROM [dbo].[Actor] WHERE IdentificationNumber = @ActorNumber",
                    new { ActorNumber = actorNumber, });
        }

        public Task<bool> ActorExistAsync(Guid actorId)
        {
            return _dbConnectionFactory
                .GetOpenConnection()
                .ExecuteScalarAsync<bool>(
                    "SELECT COUNT(1) Id FROM [dbo].[Actor] WHERE Id = @ActorId",
                    new { ActorId = actorId.ToString() });
        }
    }
}
