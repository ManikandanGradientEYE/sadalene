using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Inventory;

namespace Sadalene.Infrastructure.Data.Configurations;

public class InventorySyncLogConfiguration : IEntityTypeConfiguration<InventorySyncLog>
{
    public void Configure(EntityTypeBuilder<InventorySyncLog> builder)
    {
        builder.ToTable("InventorySyncLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.Property(x => x.TriggeredBy).HasMaxLength(100).IsRequired();
    }
}
