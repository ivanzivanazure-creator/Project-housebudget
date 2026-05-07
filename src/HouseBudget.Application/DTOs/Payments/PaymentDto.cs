using HouseBudget.Domain.Enums;

namespace HouseBudget.Application.DTOs.Payments;

public record PaymentDto(
    Guid Id,
    Guid SubscriptionId,
    string PlanName,
    decimal Amount,
    decimal? RefundedAmount,
    decimal NetAmount,
    string Currency,
    PaymentStatus Status,
    string StatusName,
    PaymentMethod Method,
    string MethodName,
    BillingPeriod BillingPeriod,
    string? CardLast4,
    string? CardBrand,
    string? InvoiceNumber,
    string? ExternalPaymentId,
    string? FailureReason,
    DateTime? PaidAt,
    DateTime? FailedAt,
    DateTime BillingPeriodStart,
    DateTime BillingPeriodEnd,
    string? BillingName,
    string? BillingEmail,
    string? BillingCountry,
    List<RefundDto> Refunds,
    DateTime CreatedAt
);

public record RefundDto(
    Guid Id,
    decimal Amount,
    string Currency,
    string Reason,
    string? ExternalRefundId,
    DateTime RefundedAt
);

public record PaymentIntentDto(
    string ClientSecret,
    string PaymentIntentId,
    decimal Amount,
    string Currency
);
