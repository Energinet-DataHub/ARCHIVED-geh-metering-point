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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.DataBaseAccess.Write.Outbox
{
    public class OutboxMessagesDataModelConfiguration : IEntityTypeConfiguration<OutboxMessagesDataModel>
    {
        public void Configure(EntityTypeBuilder<OutboxMessagesDataModel> builder)
        {
            builder.ToTable("OutboxMessages", "dbo");
            builder.HasKey(x => x.Id).HasName("PK_OutboxMessages").IsClustered(false);
            builder.Property(x => x.Id).HasColumnName(@"Id").HasColumnType("uniqueidentifier").IsRequired();
            builder.Property(x => x.RecordId).HasColumnName(@"RecordId").HasColumnType("int").IsRequired();
            builder.Property(x => x.Type).HasColumnName(@"Type").HasColumnType("nvarchar(255)").IsRequired();
            builder.Property(x => x.Data).HasColumnName(@"Data").HasColumnType("nvarchar(max)").IsRequired();
            builder.Property(x => x.Category).HasColumnName(@"Category").HasColumnType("nvarchar(255)").IsRequired();
            builder.Property(x => x.CreationDate).HasColumnName(@"CreationDate").HasColumnType("datetime2(7)")
                .IsRequired();
            builder.Property(x => x.ProcessedDate).HasColumnName(@"ProcessedDate").HasColumnType("datetime2(7)");

            builder.HasIndex(x => x.RecordId).HasDatabaseName("CIX_OutboxMessages").IsUnique();
        }
    }
}
