using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Auth;

namespace Sadalene.Infrastructure.Data.Configurations;

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.ToTable("Agents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(15).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.AgentCode).HasMaxLength(20);

        builder.HasIndex(x => x.Phone).IsUnique();
        builder.HasIndex(x => x.AgentCode).IsUnique().HasFilter("[AgentCode] IS NOT NULL");

        builder.HasMany(x => x.OtpLogs)
            .WithOne(o => o.Agent)
            .HasForeignKey(o => o.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Orders)
            .WithOne(o => o.Agent)
            .HasForeignKey(o => o.AgentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
