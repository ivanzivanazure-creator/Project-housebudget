using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record SubscriptionCreatedEvent(Guid SubscriptionId, Guid UserId, Guid PlanId, BillingPeriod BillingPeriod) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
