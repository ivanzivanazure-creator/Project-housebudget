using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record UserRegisteredEvent(Guid UserId, string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
