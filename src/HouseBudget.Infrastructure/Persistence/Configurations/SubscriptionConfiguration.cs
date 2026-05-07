using HouseBudget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBudget.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.ExternalSubscriptionId).HasMaxLength(200);
        builder.Property(s => s.ExternalCustomerId).HasMaxLength(200);
        builder.Property(s => s.CancellationReason).HasMaxLength(500);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.ExternalSubscriptionId);
        builder.HasIndex(s => new { s.UserId, s.Status });

        builder.HasOne(s => s.Plan).WithMany().HasForeignKey(s => s.PlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(s => s.Payments).WithOne().HasForeignKey(p => p.SubscriptionId).OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(s => s.IsActive);
        builder.Ignore(s => s.IsTrialing);
        builder.Ignore(s => s.DaysUntilRenewal);
        builder.Ignore(s => s.DomainEvents);
    }
}
