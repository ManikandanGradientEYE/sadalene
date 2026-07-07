using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Auth;

namespace Sadalene.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(15).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.State).HasMaxLength(100);
        builder.Property(x => x.GstNumber).HasMaxLength(20);

        // Only walk-in customers (no agent) need unique contact/email; an agent's own customers are exempt.
        builder.HasIndex(x => x.Phone).IsUnique().HasFilter("[AgentId] IS NULL");
        builder.HasIndex(x => x.Email).IsUnique().HasFilter("[AgentId] IS NULL AND [Email] IS NOT NULL");

        builder.HasOne(x => x.Agent)
            .WithMany(a => a.Customers)
            .HasForeignKey(x => x.AgentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.OtpLogs)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Invoices)
            .WithOne(i => i.Customer)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Challans)
            .WithOne(c => c.Customer)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
