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
using Energinet.DataHub.MeteringPoints.Domain.Actors;
using Energinet.DataHub.MeteringPoints.Domain.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Domain.GridAreas;
using Energinet.DataHub.MeteringPoints.Domain.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.Actors;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.EnergySuppliers;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.GridAreas;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MessageHub;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess.MeteringPoints;
using Energinet.DataHub.MeteringPoints.Infrastructure.InternalCommands;
using Energinet.DataHub.MeteringPoints.Infrastructure.LocalMessageHub;
using Energinet.DataHub.MeteringPoints.Infrastructure.Messaging.Idempotency;
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

        public DbSet<IncomingMessage> IncomingMessages { get; private set; }

        public DbSet<QueuedInternalCommand> QueuedInternalCommands { get; private set; }

        public DbSet<MessageHubMessage> MessageHubMessages { get; private set; }

        public DbSet<GridArea> GridAreas { get; private set; }

        public DbSet<EnergySupplier> EnergySuppliers { get; private set; }

        public DbSet<Actor> Actors { get; private set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.ApplyConfiguration(new OutboxMessageEntityConfiguration());
            modelBuilder.ApplyConfiguration(new MeteringPointEntityConfiguration());
            modelBuilder.ApplyConfiguration(new IncomingMessageEntityConfiguration());
            modelBuilder.ApplyConfiguration(new QueuedInternalCommandEntityConfiguration());
            modelBuilder.ApplyConfiguration(new GridAreaEntityConfiguration());
            modelBuilder.ApplyConfiguration(new MessageHubMessageEntityConfiguration());
            modelBuilder.ApplyConfiguration(new EnergySupplierEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ActorEntityConfiguration());
        }
    }
}
