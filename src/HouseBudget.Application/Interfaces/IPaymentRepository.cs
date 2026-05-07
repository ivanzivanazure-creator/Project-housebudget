using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Application.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByExternalIdAsync(string externalPaymentId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<int> GetActiveSubscriberCountAsync(CancellationToken cancellationToken = default);
}
