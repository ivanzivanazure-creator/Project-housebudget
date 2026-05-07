using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Application.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
    Task<IReadOnlyList<Account>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Account?> GetDefaultAccountAsync(Guid userId, CancellationToken cancellationToken = default);
}
