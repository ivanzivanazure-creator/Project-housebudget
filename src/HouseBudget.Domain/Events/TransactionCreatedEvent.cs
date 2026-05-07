using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record TransactionCreatedEvent(Guid TransactionId, Guid UserId, decimal Amount, TransactionType Type) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
