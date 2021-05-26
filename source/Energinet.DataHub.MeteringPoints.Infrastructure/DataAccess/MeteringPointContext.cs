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
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess
{
    public class MeteringPointContext : DbContext
    {
        #nullable disable
        public MeteringPointContext(DbContextOptions<MeteringPointContext> options)
            : base(options)
        {
        }

        public MeteringPointContext()
        {
        }

        public DbSet<OutboxMessage> OutboxMessages { get; private set; }

        public DbSet<MeteringPoint> MeteringPoints { get; private set; }

        public DbSet<ConsumptionMeteringPoint> ConsumptionMeteringPoints { get; private set; }

        public DbSet<ProductionMeteringPoint> ProductionMeteringPoints { get; private set; }

        public DbSet<ExchangeMeteringPoint> ExchangeMeteringPoints { get; private set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.ApplyConfiguration(new OutboxMessageEntityConfiguration());
            modelBuilder.ApplyConfiguration(new MeteringPointEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ConsumptionMeteringPointEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProductionMeteringPointEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ExchangeMeteringPointEntityConfiguration());
        }
    }
}
