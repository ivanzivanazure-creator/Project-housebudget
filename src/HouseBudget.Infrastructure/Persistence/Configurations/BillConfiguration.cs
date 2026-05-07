using HouseBudget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBudget.Infrastructure.Persistence.Configurations;

public sealed class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(100).IsRequired();
        builder.Property(b => b.Description).HasMaxLength(500);
        builder.Property(b => b.Color).HasMaxLength(20).IsRequired();
        builder.Property(b => b.PayeeUrl).HasMaxLength(500);

        builder.OwnsOne(b => b.Amount, mb =>
        {
            mb.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            mb.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.Ignore(b => b.IsDueSoon);
        builder.Ignore(b => b.IsOverdue);
        builder.Ignore(b => b.DomainEvents);
        builder.HasQueryFilter(b => !b.IsDeleted);

        builder.HasOne(b => b.Account).WithMany().HasForeignKey(b => b.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.Category).WithMany().HasForeignKey(b => b.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
