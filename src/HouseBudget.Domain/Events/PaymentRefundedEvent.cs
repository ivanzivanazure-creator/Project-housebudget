using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Domain.Events;

public sealed record PaymentRefundedEvent(Guid PaymentId, Guid UserId, decimal RefundAmount, string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
