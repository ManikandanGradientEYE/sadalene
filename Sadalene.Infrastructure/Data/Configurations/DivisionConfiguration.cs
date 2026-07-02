using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Masters;

namespace Sadalene.Infrastructure.Data.Configurations;

public class DivisionConfiguration : IEntityTypeConfiguration<Division>
{
    public void Configure(EntityTypeBuilder<Division> builder)
    {
        builder.ToTable("Divisions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(300);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasMany(x => x.Categories)
            .WithOne(c => c.Division)
            .HasForeignKey(c => c.DivisionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data from BRD
        builder.HasData(
            new Division { Id = 1, Name = "Lump",          Description = "Full bolt fabric ordering", IsActive = true, CreatedAt = new DateTime(2026, 6, 27) },
            new Division { Id = 2, Name = "Cutpack",       Description = "Cut piece fabric ordering",  IsActive = true, CreatedAt = new DateTime(2026, 6, 27) },
            new Division { Id = 3, Name = "Pieces",        Description = "Box/pieces ordering",        IsActive = true, CreatedAt = new DateTime(2026, 6, 27) },
            new Division { Id = 4, Name = "Make to Order", Description = "Custom length ordering",     IsActive = true, CreatedAt = new DateTime(2026, 6, 27) }
        );
    }
}
