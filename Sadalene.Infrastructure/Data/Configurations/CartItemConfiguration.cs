using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Orders;

namespace Sadalene.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).HasColumnType("decimal(18,3)").IsRequired();
        builder.Property(x => x.UnitOfMeasure).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ScannedBarcodeValue).HasMaxLength(200);
        builder.Property(x => x.UnitType).IsRequired();
        builder.Property(x => x.DisplayUnitPrice).HasColumnType("decimal(12,2)").IsRequired();
        builder.Ignore(x => x.EffectiveQuantity);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
