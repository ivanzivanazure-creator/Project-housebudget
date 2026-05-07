using HouseBudget.Domain.Enums;

namespace HouseBudget.Application.DTOs.Subscriptions;

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string? Description,
    SubscriptionTier Tier,
    string TierName,
    BillingPeriod BillingPeriod,
    string BillingPeriodName,
    decimal Price,
    string Currency,
    int TrialDays,
    bool IsActive,
    PlanFeaturesDto Features,
    string? StripePriceId
);

public record PlanFeaturesDto(
    int MaxAccounts,          // -1 = unlimited
    int MaxBudgets,
    int MaxGoals,
    int MaxCategories,
    bool CanExportData,
    bool CanAttachReceipts,
    bool HasAdvancedReports,
    bool HasBillReminders,
    bool HasEmailAlerts,
    bool HasApiAccess,
    bool HasMultiCurrency
);
