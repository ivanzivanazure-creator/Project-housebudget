using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record SubscriptionRenewedEvent(Guid SubscriptionId, Guid UserId, Guid PlanId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
