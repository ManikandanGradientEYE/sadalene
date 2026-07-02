using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sadalene.Core.Entities.Auth;

namespace Sadalene.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(15).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Role).IsRequired();

        builder.HasIndex(x => x.Phone).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");

        builder.HasMany(x => x.OtpLogs)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
