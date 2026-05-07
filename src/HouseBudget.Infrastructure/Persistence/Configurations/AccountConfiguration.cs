using HouseBudget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBudget.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(500);
        builder.Property(a => a.BankName).HasMaxLength(100);
        builder.Property(a => a.AccountNumber).HasMaxLength(50);
        builder.Property(a => a.Color).HasMaxLength(20).IsRequired();
        builder.Property(a => a.IconName).HasMaxLength(50);

        builder.OwnsOne(a => a.Balance, mb =>
        {
            mb.Property(m => m.Amount).HasColumnName("Balance").HasPrecision(18, 2);
            mb.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.OwnsOne(a => a.InitialBalance, mb =>
        {
            mb.Property(m => m.Amount).HasColumnName("InitialBalance").HasPrecision(18, 2);
            mb.Property(m => m.Currency).HasColumnName("InitialCurrency").HasMaxLength(3);
        });

        builder.Ignore(a => a.DomainEvents);
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
