using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.UserName)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(user => user.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.PhoneNumber)
            .HasMaxLength(32);

        builder.Property(user => user.TimeZoneId)
            .HasMaxLength(64);

        builder.Property(user => user.DisplayName)
            .HasMaxLength(150);

        builder.Property(user => user.PreferredCurrency)
            .HasMaxLength(3)
            .IsFixedLength();

        builder.Property(user => user.Role)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(user => user.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Ignore(user => user.FullName);
        builder.Ignore(user => user.IsAdmin);
        builder.Ignore(user => user.IsTrader);
        builder.Ignore(user => user.IsActive);

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.HasIndex(user => user.UserName)
            .IsUnique();

        builder.HasIndex(user => user.Role);
        builder.HasIndex(user => user.Status);

        builder.HasOne(user => user.Balance)
            .WithOne(balance => balance.User)
            .HasForeignKey<Balance>(balance => balance.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
