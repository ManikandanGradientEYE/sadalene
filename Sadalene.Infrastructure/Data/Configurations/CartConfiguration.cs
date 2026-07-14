using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Orders;

namespace Sadalene.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).IsRequired();

        // One active self-service cart per Customer.
        builder.HasIndex(x => x.CustomerId).IsUnique()
            .HasFilter("[CustomerId] IS NOT NULL AND [Status] = 1");

        // One active cart per (Agent, ForCustomer) pair.
        builder.HasIndex(x => new { x.AgentId, x.ForCustomerId }).IsUnique()
            .HasFilter("[AgentId] IS NOT NULL AND [Status] = 1");

        // One active shared staff/counter cart per customer — identified by having neither
        // CustomerId nor AgentId set, so any staff member can pick it up.
        builder.HasIndex(x => x.ForCustomerId).IsUnique()
            .HasFilter("[CustomerId] IS NULL AND [AgentId] IS NULL AND [Status] = 1");

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Agent)
            .WithMany()
            .HasForeignKey(x => x.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ForCustomer)
            .WithMany()
            .HasForeignKey(x => x.ForCustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LastModifiedByUser)
            .WithMany()
            .HasForeignKey(x => x.LastModifiedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Division)
            .WithMany()
            .HasForeignKey(x => x.DivisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
