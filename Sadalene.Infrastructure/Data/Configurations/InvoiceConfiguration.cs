using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Documents;

namespace Sadalene.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FileUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
    }
}
