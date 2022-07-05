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
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.WebApi
{
    public class UserProvider : IUserProvider
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger _logger;

        public UserProvider(IDbConnectionFactory connectionFactory, ILogger logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<User> GetUserAsync(Guid userId)
        {
            var sql = @"SELECT
                u.Id,
                ua.ActorId
                FROM [dbo].[User] u
                JOIN [dbo].[UserActor] ua on u.Id = ua.UserId
                WHERE u.Id = @UserId";

            try
            {
                var result = await _connectionFactory
                    .GetOpenConnection()
                    .QueryAsync<(Guid, Guid)>(sql, new { UserId = userId })
                    .ConfigureAwait(false);

                var actorIds = result.Select(row => row.Item2).ToList();

                return new(userId, actorIds);
            }
            catch (Exception)
            {
                _logger.LogInformation("GetUserAsync {UserId}", userId);
                throw;
            }
        }
    }
}
