using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Application.Interfaces;

public interface IBillRepository : IRepository<Bill>
{
    Task<IReadOnlyList<Bill>> GetByUserIdAsync(Guid userId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetUpcomingBillsAsync(Guid userId, int days = 7, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetOverdueBillsAsync(Guid userId, CancellationToken cancellationToken = default);
}
