using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record PaymentFailedEvent(Guid PaymentId, Guid UserId, Guid SubscriptionId, string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
