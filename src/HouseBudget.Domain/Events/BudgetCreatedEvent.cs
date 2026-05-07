using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record BudgetCreatedEvent(Guid BudgetId, Guid UserId, decimal TotalAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
