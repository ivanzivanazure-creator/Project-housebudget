namespace HouseBudget.Mobile.Models;

public record SubscriptionPlanModel(
    Guid Id,
    string Name,
    string Tier,
    string BillingPeriod,
    decimal Price,
    string Currency,
    string Description,
    int TrialDays,
    PlanFeaturesModel Features
);

public record PlanFeaturesModel(
    int MaxAccounts,
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

public record SubscriptionModel(
    Guid Id,
    string TierName,
    string StatusName,
    string BillingPeriodName,
    decimal Price,
    string Currency,
    bool IsTrialing,
    bool IsActive,
    DateTime? TrialEndsAt,
    DateTime CurrentPeriodStart,
    DateTime CurrentPeriodEnd,
    int DaysUntilRenewal,
    string PlanName,
    PlanFeaturesModel Features
);

public record PaymentHistoryItem(
    Guid Id,
    string InvoiceNumber,
    decimal Amount,
    string Currency,
    string StatusName,
    string PlanName,
    DateTime PaidAt
);
