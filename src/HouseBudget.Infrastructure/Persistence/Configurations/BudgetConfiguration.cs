using HouseBudget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBudget.Infrastructure.Persistence.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(100).IsRequired();
        builder.Property(b => b.Description).HasMaxLength(500);

        builder.OwnsOne(b => b.TotalAmount, mb =>
        {
            mb.Property(m => m.Amount).HasColumnName("TotalAmount").HasPrecision(18, 2);
            mb.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.OwnsOne(b => b.Period, dr =>
        {
            dr.Property(p => p.Start).HasColumnName("PeriodStart");
            dr.Property(p => p.End).HasColumnName("PeriodEnd");
        });

        builder.Ignore(b => b.TotalAllocated);
        builder.Ignore(b => b.TotalSpent);
        builder.Ignore(b => b.Remaining);
        builder.Ignore(b => b.UsagePercentage);
        builder.Ignore(b => b.IsOverBudget);
        builder.Ignore(b => b.DomainEvents);
        builder.HasQueryFilter(b => !b.IsDeleted);

        builder.HasMany(b => b.BudgetCategories).WithOne(bc => bc.Budget).HasForeignKey(bc => bc.BudgetId).OnDelete(DeleteBehavior.Cascade);
    }
}
