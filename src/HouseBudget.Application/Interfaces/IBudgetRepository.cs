using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Application.Interfaces;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<IReadOnlyList<Budget>> GetByUserIdAsync(Guid userId, bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<Budget?> GetWithCategoriesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Budget>> GetActiveBudgetsForDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default);
}
