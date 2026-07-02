using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Masters;

namespace Sadalene.Infrastructure.Data.Configurations;

public class DivisionUnitOfMeasureConfiguration : IEntityTypeConfiguration<DivisionUnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<DivisionUnitOfMeasure> builder)
    {
        builder.ToTable("DivisionUnitOfMeasures");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UnitName).HasMaxLength(50).IsRequired();

        // Seed data from BRD Division-wise UOM table
        builder.HasData(
            new DivisionUnitOfMeasure { Id = 1, DivisionId = 1, UnitName = "Full Set",     IsDefault = true,  IsActive = true, CreatedAt = new DateTime(2026, 6, 27) },
            new DivisionUnitOfMeasure { Id = 2, DivisionId = 1, UnitName = "Half Set",     IsDefault = false, IsActive = true, CreatedAt = new DateTime(2026, 6, 27) },
            new DivisionUnitOfMeasure { Id = 3, DivisionId = 2, UnitName = "Taka",         IsDefault = true,  IsActive = true, CreatedAt = new DateTime(2026, 6, 27) },
            new DivisionUnitOfMeasure { Id = 4, DivisionId = 3, UnitName = "No. of Boxes", IsDefault = true,  IsActive = true, CreatedAt = new DateTime(2026, 6, 27) },
            new DivisionUnitOfMeasure { Id = 5, DivisionId = 4, UnitName = "Meters",       IsDefault = true,  IsActive = true, CreatedAt = new DateTime(2026, 6, 27) }
        );
    }
}
