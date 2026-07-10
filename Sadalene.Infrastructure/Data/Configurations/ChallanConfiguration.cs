using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Documents;

namespace Sadalene.Infrastructure.Data.Configurations;

public class ChallanConfiguration : IEntityTypeConfiguration<Challan>
{
    public void Configure(EntityTypeBuilder<Challan> builder)
    {
        builder.ToTable("Challans");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChallanNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FileUrl).HasMaxLength(500);

        builder.HasIndex(x => x.ChallanNumber).IsUnique();
    }
}
