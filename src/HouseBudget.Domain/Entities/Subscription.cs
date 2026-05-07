using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Events;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;

namespace HouseBudget.Domain.Entities;

public sealed class Subscription : BaseEntity
{
    private readonly List<Payment> _payments = new();

    public Guid UserId { get; private set; }
    public Guid PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public BillingPeriod BillingPeriod { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    public DateTime CurrentPeriodStart { get; private set; }
    public DateTime CurrentPeriodEnd { get; private set; }
    public bool AutoRenew { get; private set; } = true;
    public bool CancelAtPeriodEnd { get; private set; }

    // Stripe / gateway references
    public string? ExternalSubscriptionId { get; private set; }
    public string? ExternalCustomerId { get; private set; }

    // Cancellation
    public string? CancellationReason { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public SubscriptionPlan Plan { get; private set; } = default!;
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    public bool IsActive => Status is SubscriptionStatus.Active or SubscriptionStatus.Trialing;
    public bool IsTrialing => Status == SubscriptionStatus.Trialing && TrialEndDate.HasValue && TrialEndDate.Value > DateTime.UtcNow;
    public int DaysUntilRenewal => IsActive ? Math.Max(0, (int)(CurrentPeriodEnd - DateTime.UtcNow).TotalDays) : 0;

    private Subscription() { }

    public static Subscription Create(Guid userId, Guid planId, BillingPeriod billingPeriod, int trialDays = 0)
    {
        var now = DateTime.UtcNow;
        var isTrialing = trialDays > 0;

        var sub = new Subscription
        {
            UserId = userId,
            PlanId = planId,
            BillingPeriod = billingPeriod,
            Status = isTrialing ? SubscriptionStatus.Trialing : SubscriptionStatus.Active,
            StartDate = now,
            TrialEndDate = isTrialing ? now.AddDays(trialDays) : null,
            CurrentPeriodStart = now,
            CurrentPeriodEnd = billingPeriod == BillingPeriod.Monthly ? now.AddMonths(1) : now.AddYears(1)
        };

        sub.AddDomainEvent(new SubscriptionCreatedEvent(sub.Id, userId, planId, billingPeriod));
        return sub;
    }

    public void Activate(string? externalSubId = null, string? externalCustomerId = null)
    {
        Status = SubscriptionStatus.Active;
        ExternalSubscriptionId = externalSubId;
        ExternalCustomerId = externalCustomerId;
        MarkUpdated();
    }

    public void Renew()
    {
        if (!IsActive) throw new DomainException("Cannot renew an inactive subscription.");
        CurrentPeriodStart = CurrentPeriodEnd;
        CurrentPeriodEnd = BillingPeriod == BillingPeriod.Monthly
            ? CurrentPeriodEnd.AddMonths(1)
            : CurrentPeriodEnd.AddYears(1);
        Status = SubscriptionStatus.Active;
        AddDomainEvent(new SubscriptionRenewedEvent(Id, UserId, PlanId));
        MarkUpdated();
    }

    public void Cancel(string? reason = null, bool atPeriodEnd = true)
    {
        if (Status == SubscriptionStatus.Cancelled) throw new DomainException("Subscription is already cancelled.");
        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;
        if (atPeriodEnd)
        {
            CancelAtPeriodEnd = true;
            AutoRenew = false;
        }
        else
        {
            Status = SubscriptionStatus.Cancelled;
            EndDate = DateTime.UtcNow;
        }
        AddDomainEvent(new SubscriptionCancelledEvent(Id, UserId, reason));
        MarkUpdated();
    }

    public void Upgrade(Guid newPlanId)
    {
        if (!IsActive) throw new DomainException("Cannot upgrade an inactive subscription.");
        PlanId = newPlanId;
        CancelAtPeriodEnd = false;
        AutoRenew = true;
        AddDomainEvent(new SubscriptionUpgradedEvent(Id, UserId, newPlanId));
        MarkUpdated();
    }

    public void MarkPastDue() { Status = SubscriptionStatus.PastDue; MarkUpdated(); }
    public void Expire() { Status = SubscriptionStatus.Expired; EndDate = DateTime.UtcNow; AddDomainEvent(new SubscriptionExpiredEvent(Id, UserId)); MarkUpdated(); }
    public void Pause() { Status = SubscriptionStatus.Paused; MarkUpdated(); }
    public void Resume() { Status = SubscriptionStatus.Active; MarkUpdated(); }
    public void SetExternalIds(string subId, string customerId) { ExternalSubscriptionId = subId; ExternalCustomerId = customerId; MarkUpdated(); }
}
