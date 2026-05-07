using HouseBudget.Application.DTOs.Subscriptions;
using HouseBudget.Domain.Entities;

namespace HouseBudget.Application.Commands.Subscriptions;

public static class SubscriptionMapper
{
    public static SubscriptionDto ToDto(Subscription s, SubscriptionPlan plan) => new(
        s.Id, plan.Id, plan.Name, plan.Tier, plan.Tier.ToString(),
        s.Status, s.Status.ToString(), s.BillingPeriod,
        plan.Price.Amount, plan.Price.Currency,
        s.StartDate, s.EndDate, s.TrialEndDate,
        s.CurrentPeriodStart, s.CurrentPeriodEnd,
        s.AutoRenew, s.CancelAtPeriodEnd, s.IsActive, s.IsTrialing,
        s.DaysUntilRenewal, s.CancellationReason, s.CancelledAt,
        ToPlanFeatures(plan));

    public static SubscriptionPlanDto ToPlanDto(SubscriptionPlan p) => new(
        p.Id, p.Name, p.Description, p.Tier, p.Tier.ToString(),
        p.BillingPeriod, p.BillingPeriod.ToString(),
        p.Price.Amount, p.Price.Currency,
        p.TrialDays, p.IsActive, ToPlanFeatures(p), p.StripePriceId);

    private static PlanFeaturesDto ToPlanFeatures(SubscriptionPlan p) => new(
        p.MaxAccounts, p.MaxBudgets, p.MaxGoals, p.MaxCategories,
        p.CanExportData, p.CanAttachReceipts, p.HasAdvancedReports,
        p.HasBillReminders, p.HasEmailAlerts, p.HasApiAccess, p.HasMultiCurrency);
}
