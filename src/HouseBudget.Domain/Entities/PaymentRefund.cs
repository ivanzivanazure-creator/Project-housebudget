using HouseBudget.Domain.ValueObjects;

namespace HouseBudget.Domain.Entities;

public sealed class PaymentRefund : BaseEntity
{
    public Guid PaymentId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public string Reason { get; private set; } = default!;
    public string? ExternalRefundId { get; private set; }
    public DateTime RefundedAt { get; private set; }

    public Payment Payment { get; private set; } = default!;

    private PaymentRefund() { }

    public static PaymentRefund Create(Guid paymentId, decimal amount, string currency, string reason)
        => new()
        {
            PaymentId = paymentId,
            Amount = Money.Of(amount, currency),
            Reason = reason,
            RefundedAt = DateTime.UtcNow
        };

    public void SetExternalRefundId(string id) { ExternalRefundId = id; MarkUpdated(); }
}
