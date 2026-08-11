using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Persistence.Configurations;

public class BalanceConfiguration : IEntityTypeConfiguration<Balance>
{
    public void Configure(EntityTypeBuilder<Balance> builder)
    {
        builder.ToTable("Balances");

        builder.HasKey(balance => balance.Id);

        builder.Property(balance => balance.Currency)
            .HasMaxLength(3)
            .IsFixedLength()
            .IsRequired();

        builder.Property(balance => balance.Available)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(balance => balance.Reserved)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Ignore(balance => balance.Total);
        builder.Ignore(balance => balance.AvailableMoney);
        builder.Ignore(balance => balance.ReservedMoney);
        builder.Ignore(balance => balance.TotalMoney);

        builder.HasIndex(balance => balance.UserId)
            .IsUnique();
    }
}
