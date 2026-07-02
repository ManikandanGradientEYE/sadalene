using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Inventory;

namespace Sadalene.Infrastructure.Data.Configurations;

public class InventoryRecordConfiguration : IEntityTypeConfiguration<InventoryRecord>
{
    public void Configure(EntityTypeBuilder<InventoryRecord> builder)
    {
        builder.ToTable("InventoryRecords");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuantityAvailable).HasColumnType("decimal(18,3)").IsRequired();
        builder.Property(x => x.UnitOfMeasure).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SyncSource).HasMaxLength(100);

        builder.HasIndex(x => x.ProductId).IsUnique();
    }
}
