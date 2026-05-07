using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record BudgetExceededEvent(Guid BudgetId, Guid UserId, decimal BudgetAmount, decimal SpentAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
