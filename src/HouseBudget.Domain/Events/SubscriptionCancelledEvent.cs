using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record SubscriptionCancelledEvent(Guid SubscriptionId, Guid UserId, string? Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
