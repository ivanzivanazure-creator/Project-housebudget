using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Application.Interfaces;

public interface IGoalRepository : IRepository<Goal>
{
    Task<IReadOnlyList<Goal>> GetByUserIdAsync(Guid userId, GoalStatus? status = null, CancellationToken cancellationToken = default);
}
