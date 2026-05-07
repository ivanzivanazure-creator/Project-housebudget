using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Application.Interfaces;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subscription>> GetExpiringSoonAsync(int days, CancellationToken cancellationToken = default);
}
