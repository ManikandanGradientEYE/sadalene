using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Orders;

namespace Sadalene.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.Status).IsRequired();

        builder.HasIndex(x => x.OrderNumber).IsUnique();

        builder.HasMany(x => x.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Admin placed order — no cascade on User delete
        builder.HasOne(x => x.PlacedByUser)
            .WithMany()
            .HasForeignKey(x => x.PlacedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
