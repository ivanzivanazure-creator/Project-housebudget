using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record PaymentCompletedEvent(Guid PaymentId, Guid UserId, Guid SubscriptionId, decimal Amount, string Currency) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
