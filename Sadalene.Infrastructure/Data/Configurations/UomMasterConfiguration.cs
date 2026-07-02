using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Masters;

namespace Sadalene.Infrastructure.Data.Configurations;

public class UomMasterConfiguration : IEntityTypeConfiguration<UomMaster>
{
    public void Configure(EntityTypeBuilder<UomMaster> builder)
    {
        builder.ToTable("UomMasters");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Abbreviation).HasMaxLength(20);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasMany(x => x.SubCategories)
            .WithOne(s => s.UomMaster)
            .HasForeignKey(s => s.UomMasterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Products)
            .WithOne(p => p.UomMaster)
            .HasForeignKey(p => p.UomMasterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
