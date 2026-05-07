using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Application.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IReadOnlyList<Transaction>> GetByUserIdAsync(Guid userId, DateOnly? from = null, DateOnly? to = null, Guid? categoryId = null, TransactionType? type = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalByTypeAsync(Guid userId, TransactionType type, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, decimal>> GetSpendingByCategoryAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetRecentAsync(Guid userId, int count, CancellationToken cancellationToken = default);
}
