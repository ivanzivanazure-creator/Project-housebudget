using HouseBudget.Domain.Enums;

namespace HouseBudget.Application.DTOs.Subscriptions;

public record SubscriptionDto(
    Guid Id,
    Guid PlanId,
    string PlanName,
    SubscriptionTier Tier,
    string TierName,
    SubscriptionStatus Status,
    string StatusName,
    BillingPeriod BillingPeriod,
    decimal Price,
    string Currency,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime? TrialEndDate,
    DateTime CurrentPeriodStart,
    DateTime CurrentPeriodEnd,
    bool AutoRenew,
    bool CancelAtPeriodEnd,
    bool IsActive,
    bool IsTrialing,
    int DaysUntilRenewal,
    string? CancellationReason,
    DateTime? CancelledAt,
    PlanFeaturesDto Features
);
