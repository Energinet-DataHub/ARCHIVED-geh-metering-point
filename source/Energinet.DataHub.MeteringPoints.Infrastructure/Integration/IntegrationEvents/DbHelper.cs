using System;
using System.Threading.Tasks;
using Dapper;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Integration.Helpers
{
    public class DbHelper
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DbHelper(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<string> GetGridAreaCodeAsync(Guid gridAreaLinkId)
        {
            var sql = @"SELECT GridAreas.Code FROM GridAreas
                        INNER JOIN GridAreaLinks ON GridAreas.Id = GridAreaLinks.GridAreaId
                        WHERE GridAreaLinks.Id =@GridAreaLinkId";
            var result = await _connectionFactory
                .GetOpenConnection()
                .ExecuteScalarAsync<string?>(sql, new { gridAreaLinkId })
                .ConfigureAwait(false);

            return result ?? throw new InvalidOperationException("Grid Area Code not found");
        }
    }
}
