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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Database;
using Energinet.DataHub.MeteringPoints.ApplyDBMigrationsApp.Helpers;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.IntegrationTests.Fixtures
{
    public class MeteringPointDatabaseManager : SqlServerDatabaseManager<MeteringPointContext>
    {
        public MeteringPointDatabaseManager()
            : base("MeteringPoint")
        {
        }

        /// <inheritdoc/>
        public override MeteringPointContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<MeteringPointContext>();
            var dbContextOptions = optionsBuilder
                .UseSqlServer(ConnectionString, options => options.UseNodaTime());

            return new MeteringPointContext(optionsBuilder.Options);
        }

        /// <summary>
        /// Creates the database schema using DbUp instead of a database context.
        /// </summary>
        protected override Task<bool> CreateDatabaseSchemaAsync(MeteringPointContext context)
        {
            return Task.FromResult(CreateDatabaseSchema(context));
        }

        /// <summary>
        /// Creates the database schema using DbUp instead of a database context.
        /// </summary>
        protected override bool CreateDatabaseSchema(MeteringPointContext context)
        {
            var result = DefaultUpgrader.Upgrade(ConnectionString);
            if (result.Successful is false)
                throw new InvalidOperationException("Database migration failed", result.Error);

            return true;
        }
    }
}
