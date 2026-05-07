namespace HouseBudget.Domain.Interfaces;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
    Guid EventId { get; }
}
