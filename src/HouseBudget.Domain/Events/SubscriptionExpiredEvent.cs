using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record SubscriptionExpiredEvent(Guid SubscriptionId, Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
