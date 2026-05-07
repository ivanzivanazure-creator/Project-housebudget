using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record GoalAchievedEvent(Guid GoalId, Guid UserId, string GoalName, decimal TargetAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
