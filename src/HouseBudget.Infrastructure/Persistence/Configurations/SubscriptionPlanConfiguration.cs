using HouseBudget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBudget.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.StripePriceId).HasMaxLength(100);
        builder.Property(p => p.StripeProductId).HasMaxLength(100);

        builder.OwnsOne(p => p.Price, mb =>
        {
            mb.Property(m => m.Amount).HasColumnName("Price").HasPrecision(18, 2);
            mb.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.HasIndex(p => new { p.Tier, p.BillingPeriod });
        builder.Ignore(p => p.DomainEvents);
    }
}
