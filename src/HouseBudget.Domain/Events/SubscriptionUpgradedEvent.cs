using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record SubscriptionUpgradedEvent(Guid SubscriptionId, Guid UserId, Guid NewPlanId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
