using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Products;

namespace Sadalene.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MarketName).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ProductCode).HasMaxLength(50);

        // Pricing
        builder.Property(x => x.Rate).HasPrecision(12, 2);
        builder.Property(x => x.RatePer).HasMaxLength(50);
        builder.Property(x => x.Cut).HasPrecision(10, 3);
        builder.Property(x => x.QtyPerUnit).HasPrecision(12, 3);
        builder.Property(x => x.Grade).HasMaxLength(50);

        // Specs
        builder.Property(x => x.FabricComposition).HasMaxLength(200);
        builder.Property(x => x.Width).HasMaxLength(50);
        builder.Property(x => x.Weight).HasMaxLength(50);
        builder.Property(x => x.Color).HasMaxLength(100);
        builder.Property(x => x.DesignNo).HasMaxLength(50);
        builder.Property(x => x.Design).HasMaxLength(200);
        builder.Property(x => x.Brand).HasMaxLength(100);
        builder.Property(x => x.BarcodeValue).HasMaxLength(200);
        builder.Property(x => x.BarcodeImageUrl).HasMaxLength(500);

        builder.HasIndex(x => x.ProductCode).IsUnique().HasFilter("[ProductCode] IS NOT NULL");
        builder.HasIndex(x => x.BarcodeValue).IsUnique().HasFilter("[BarcodeValue] IS NOT NULL");

        builder.HasMany(x => x.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.InventoryRecords)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.OrderItems)
            .WithOne(oi => oi.Product)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
