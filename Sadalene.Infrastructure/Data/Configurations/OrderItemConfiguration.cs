using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Orders;

namespace Sadalene.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).HasColumnType("decimal(18,3)").IsRequired();
        builder.Property(x => x.UnitOfMeasure).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ScannedBarcodeValue).HasMaxLength(200);
        builder.Property(x => x.UnitType).IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(12,2)").IsRequired();
        builder.Ignore(x => x.EffectiveQuantity);
        builder.Ignore(x => x.LineTotal);
    }
}
