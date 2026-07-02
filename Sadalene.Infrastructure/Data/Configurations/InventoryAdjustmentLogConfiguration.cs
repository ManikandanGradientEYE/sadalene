using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Inventory;

namespace Sadalene.Infrastructure.Data.Configurations;

public class InventoryAdjustmentLogConfiguration : IEntityTypeConfiguration<InventoryAdjustmentLog>
{
    public void Configure(EntityTypeBuilder<InventoryAdjustmentLog> builder)
    {
        builder.ToTable("InventoryAdjustmentLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AdjustmentType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.PreviousQuantity).HasPrecision(18, 3);
        builder.Property(x => x.NewQuantity).HasPrecision(18, 3);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.AdjustedBy).HasMaxLength(200).IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.AdjustedAt);
        builder.HasIndex(x => x.ProductId);
    }
}
