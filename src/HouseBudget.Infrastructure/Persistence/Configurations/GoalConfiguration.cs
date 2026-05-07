using HouseBudget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBudget.Infrastructure.Persistence.Configurations;

public sealed class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
        builder.Property(g => g.Description).HasMaxLength(500);
        builder.Property(g => g.Color).HasMaxLength(20).IsRequired();
        builder.Property(g => g.IconName).HasMaxLength(50);

        builder.OwnsOne(g => g.TargetAmount, mb =>
        {
            mb.Property(m => m.Amount).HasColumnName("TargetAmount").HasPrecision(18, 2);
            mb.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.OwnsOne(g => g.CurrentAmount, mb =>
        {
            mb.Property(m => m.Amount).HasColumnName("CurrentAmount").HasPrecision(18, 2);
            mb.Property(m => m.Currency).HasColumnName("CurrentCurrency").HasMaxLength(3);
        });

        builder.Ignore(g => g.ProgressPercentage);
        builder.Ignore(g => g.IsCompleted);
        builder.Ignore(g => g.Remaining);
        builder.Ignore(g => g.MonthlyRequired);
        builder.Ignore(g => g.DomainEvents);
        builder.HasQueryFilter(g => !g.IsDeleted);

        builder.HasMany(g => g.Contributions).WithOne(gc => gc.Goal).HasForeignKey(gc => gc.GoalId).OnDelete(DeleteBehavior.Cascade);
    }
}
