using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record BillPaidEvent(Guid BillId, Guid UserId, string BillName, decimal Amount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
