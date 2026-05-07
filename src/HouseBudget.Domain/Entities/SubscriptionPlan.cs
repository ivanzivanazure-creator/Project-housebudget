using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;

namespace HouseBudget.Domain.Entities;

public sealed class SubscriptionPlan : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public SubscriptionTier Tier { get; private set; }
    public BillingPeriod BillingPeriod { get; private set; }
    public Money Price { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public string? StripePriceId { get; private set; }
    public string? StripeProductId { get; private set; }
    public int TrialDays { get; private set; }
    public int SortOrder { get; private set; }

    // Feature limits (-1 = unlimited)
    public int MaxAccounts { get; private set; }
    public int MaxBudgets { get; private set; }
    public int MaxGoals { get; private set; }
    public int MaxCategories { get; private set; }
    public bool CanExportData { get; private set; }
    public bool CanAttachReceipts { get; private set; }
    public bool HasAdvancedReports { get; private set; }
    public bool HasBillReminders { get; private set; }
    public bool HasEmailAlerts { get; private set; }
    public bool HasApiAccess { get; private set; }
    public bool HasMultiCurrency { get; private set; }

    private SubscriptionPlan() { }

    public static SubscriptionPlan Create(
        string name,
        SubscriptionTier tier,
        BillingPeriod billingPeriod,
        decimal price,
        string currency,
        FeatureLimits limits,
        string? description = null,
        int trialDays = 0,
        string? stripePriceId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Plan name is required.");

        return new SubscriptionPlan
        {
            Name = name,
            Description = description,
            Tier = tier,
            BillingPeriod = billingPeriod,
            Price = Money.Of(price, currency),
            TrialDays = trialDays,
            StripePriceId = stripePriceId,
            MaxAccounts = limits.MaxAccounts,
            MaxBudgets = limits.MaxBudgets,
            MaxGoals = limits.MaxGoals,
            MaxCategories = limits.MaxCategories,
            CanExportData = limits.CanExportData,
            CanAttachReceipts = limits.CanAttachReceipts,
            HasAdvancedReports = limits.HasAdvancedReports,
            HasBillReminders = limits.HasBillReminders,
            HasEmailAlerts = limits.HasEmailAlerts,
            HasApiAccess = limits.HasApiAccess,
            HasMultiCurrency = limits.HasMultiCurrency
        };
    }

    public void UpdateStripeIds(string priceId, string productId)
    {
        StripePriceId = priceId;
        StripeProductId = productId;
        MarkUpdated();
    }

    public void Deactivate() { IsActive = false; MarkUpdated(); }

    public decimal GetAnnualSavings()
    {
        if (BillingPeriod == BillingPeriod.Annual) return 0;
        var monthlyAnnualCost = Price.Amount * 12;
        return 0; // compare with annual plan externally
    }
}

public record FeatureLimits(
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
)
{
    public static FeatureLimits Free => new(2, 1, 2, 10, false, false, false, false, false, false, false);
    public static FeatureLimits Premium => new(-1, -1, -1, -1, true, true, true, true, true, false, true);
    public static FeatureLimits Business => new(-1, -1, -1, -1, true, true, true, true, true, true, true);
}
