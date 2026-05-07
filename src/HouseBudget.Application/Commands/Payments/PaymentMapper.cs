using HouseBudget.Application.DTOs.Payments;
using HouseBudget.Domain.Entities;

namespace HouseBudget.Application.Commands.Payments;

public static class PaymentMapper
{
    public static PaymentDto ToDto(Payment p, string planName) => new(
        p.Id, p.SubscriptionId, planName,
        p.Amount.Amount, p.RefundedAmount?.Amount, p.NetAmount.Amount, p.Amount.Currency,
        p.Status, p.Status.ToString(), p.Method, p.Method.ToString(), p.BillingPeriod,
        p.CardLast4, p.CardBrand, p.InvoiceNumber, p.ExternalPaymentId, p.FailureReason,
        p.PaidAt, p.FailedAt, p.BillingPeriodStart, p.BillingPeriodEnd,
        p.BillingName, p.BillingEmail, p.BillingCountry,
        p.Refunds.Select(r => new RefundDto(r.Id, r.Amount.Amount, r.Amount.Currency, r.Reason, r.ExternalRefundId, r.RefundedAt)).ToList(),
        p.CreatedAt);
}
