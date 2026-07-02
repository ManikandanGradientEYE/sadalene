using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Auth;

namespace Sadalene.Infrastructure.Data.Configurations;

public class OtpLogConfiguration : IEntityTypeConfiguration<OtpLog>
{
    public void Configure(EntityTypeBuilder<OtpLog> builder)
    {
        builder.ToTable("OtpLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Phone).HasMaxLength(15).IsRequired();
        builder.Property(x => x.OtpCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Purpose).IsRequired();

        builder.HasIndex(x => new { x.Phone, x.OtpCode, x.IsUsed });
    }
}
