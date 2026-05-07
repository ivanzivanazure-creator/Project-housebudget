using HouseBudget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBudget.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Description).HasMaxLength(500).IsRequired();
        builder.Property(t => t.Notes).HasMaxLength(1000);
        builder.Property(t => t.Merchant).HasMaxLength(200);
        builder.Property(t => t.Location).HasMaxLength(300);
        builder.Property(t => t.ReceiptImageUrl).HasMaxLength(500);

        builder.OwnsOne(t => t.Amount, mb =>
        {
            mb.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            mb.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.Property(t => t.Tags).HasConversion(
            v => string.Join(',', v),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.TransactionDate);
        builder.HasIndex(t => new { t.UserId, t.TransactionDate });

        builder.Ignore(t => t.DomainEvents);
        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.HasOne(t => t.Account).WithMany().HasForeignKey(t => t.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Category).WithMany().HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
