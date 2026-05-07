using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Events;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;

namespace HouseBudget.Domain.Entities;

public sealed class Payment : BaseEntity
{
    private readonly List<PaymentRefund> _refunds = new();

    public Guid UserId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public Guid PlanId { get; private set; }

    public Money Amount { get; private set; } = default!;
    public Money? RefundedAmount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }
    public BillingPeriod BillingPeriod { get; private set; }

    // Gateway
    public string? ExternalPaymentId { get; private set; }
    public string? ExternalInvoiceId { get; private set; }
    public string? GatewayResponse { get; private set; }

    // Card details (masked)
    public string? CardLast4 { get; private set; }
    public string? CardBrand { get; private set; }
    public string? CardExpiry { get; private set; }

    // Billing address
    public string? BillingName { get; private set; }
    public string? BillingEmail { get; private set; }
    public string? BillingCountry { get; private set; }

    // Invoice
    public string? InvoiceNumber { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime BillingPeriodStart { get; private set; }
    public DateTime BillingPeriodEnd { get; private set; }
    public int RetryCount { get; private set; }

    public IReadOnlyCollection<PaymentRefund> Refunds => _refunds.AsReadOnly();
    public Money NetAmount => RefundedAmount is not null ? Amount.Subtract(RefundedAmount) : Amount;
    public bool IsRefundable => Status == PaymentStatus.Completed && _refunds.Sum(r => r.Amount.Amount) < Amount.Amount;

    private Payment() { }

    public static Payment Create(
        Guid userId,
        Guid subscriptionId,
        Guid planId,
        decimal amount,
        string currency,
        PaymentMethod method,
        BillingPeriod billingPeriod,
        DateTime periodStart,
        DateTime periodEnd,
        string? externalPaymentId = null,
        string? invoiceNumber = null)
    {
        if (amount <= 0) throw new DomainException("Payment amount must be positive.");

        return new Payment
        {
            UserId = userId,
            SubscriptionId = subscriptionId,
            PlanId = planId,
            Amount = Money.Of(amount, currency),
            Status = PaymentStatus.Pending,
            Method = method,
            BillingPeriod = billingPeriod,
            ExternalPaymentId = externalPaymentId,
            InvoiceNumber = invoiceNumber ?? GenerateInvoiceNumber(),
            BillingPeriodStart = periodStart,
            BillingPeriodEnd = periodEnd
        };
    }

    public void MarkCompleted(string? gatewayResponse = null, string? cardLast4 = null, string? cardBrand = null)
    {
        Status = PaymentStatus.Completed;
        PaidAt = DateTime.UtcNow;
        GatewayResponse = gatewayResponse;
        CardLast4 = cardLast4;
        CardBrand = cardBrand;
        AddDomainEvent(new PaymentCompletedEvent(Id, UserId, SubscriptionId, Amount.Amount, Amount.Currency));
        MarkUpdated();
    }

    public void MarkFailed(string reason, string? gatewayResponse = null)
    {
        Status = PaymentStatus.Failed;
        FailedAt = DateTime.UtcNow;
        FailureReason = reason;
        GatewayResponse = gatewayResponse;
        RetryCount++;
        AddDomainEvent(new PaymentFailedEvent(Id, UserId, SubscriptionId, reason));
        MarkUpdated();
    }

    public void MarkProcessing(string externalPaymentId)
    {
        Status = PaymentStatus.Processing;
        ExternalPaymentId = externalPaymentId;
        MarkUpdated();
    }

    public PaymentRefund Refund(decimal amount, string reason)
    {
        if (!IsRefundable) throw new DomainException("This payment cannot be refunded.");

        var totalRefunded = _refunds.Sum(r => r.Amount.Amount) + amount;
        if (totalRefunded > Amount.Amount) throw new DomainException("Refund amount exceeds original payment.");

        var refund = PaymentRefund.Create(Id, amount, Amount.Currency, reason);
        _refunds.Add(refund);
        RefundedAmount = Money.Of(totalRefunded, Amount.Currency);
        Status = totalRefunded == Amount.Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;

        AddDomainEvent(new PaymentRefundedEvent(Id, UserId, amount, reason));
        MarkUpdated();
        return refund;
    }

    public void SetBillingDetails(string? name, string? email, string? country, string? externalInvoiceId)
    {
        BillingName = name;
        BillingEmail = email;
        BillingCountry = country;
        ExternalInvoiceId = externalInvoiceId;
        MarkUpdated();
    }

    private static string GenerateInvoiceNumber()
        => $"INV-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}
