using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Masters;

namespace Sadalene.Infrastructure.Data.Configurations;

public class PackingTypeConfiguration : IEntityTypeConfiguration<PackingType>
{
    public void Configure(EntityTypeBuilder<PackingType> builder)
    {
        builder.ToTable("PackingTypes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasMany(x => x.Products)
            .WithOne(p => p.PackingType)
            .HasForeignKey(p => p.PackingTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
