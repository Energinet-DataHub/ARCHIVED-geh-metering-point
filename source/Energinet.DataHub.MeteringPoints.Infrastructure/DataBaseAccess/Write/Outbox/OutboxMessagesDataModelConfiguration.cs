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
