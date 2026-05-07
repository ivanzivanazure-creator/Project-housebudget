using HouseBudget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBudget.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ExternalPaymentId).HasMaxLength(200);
        builder.Property(p => p.ExternalInvoiceId).HasMaxLength(200);
        builder.Property(p => p.GatewayResponse).HasMaxLength(2000);
        builder.Property(p => p.CardLast4).HasMaxLength(4);
        builder.Property(p => p.CardBrand).HasMaxLength(20);
        builder.Property(p => p.CardExpiry).HasMaxLength(7);
        builder.Property(p => p.InvoiceNumber).HasMaxLength(50);
        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.BillingName).HasMaxLength(200);
        builder.Property(p => p.BillingEmail).HasMaxLength(200);
        builder.Property(p => p.BillingCountry).HasMaxLength(2);

        builder.OwnsOne(p => p.Amount, mb =>
        {
            mb.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            mb.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.OwnsOne(p => p.RefundedAmount, mb =>
        {
            mb.Property(m => m.Amount).HasColumnName("RefundedAmount").HasPrecision(18, 2);
            mb.Property(m => m.Currency).HasColumnName("RefundedCurrency").HasMaxLength(3);
        });

        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.ExternalPaymentId);
        builder.HasIndex(p => p.InvoiceNumber).IsUnique();
        builder.HasIndex(p => new { p.UserId, p.Status });

        builder.HasMany(p => p.Refunds).WithOne(r => r.Payment).HasForeignKey(r => r.PaymentId).OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(p => p.NetAmount);
        builder.Ignore(p => p.IsRefundable);
        builder.Ignore(p => p.DomainEvents);
    }
}
