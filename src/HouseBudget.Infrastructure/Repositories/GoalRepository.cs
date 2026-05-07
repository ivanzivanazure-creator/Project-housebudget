using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseBudget.Infrastructure.Repositories;

public sealed class GoalRepository : BaseRepository<Goal>, IGoalRepository
{
    public GoalRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Goal>> GetByUserIdAsync(Guid userId, GoalStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(g => g.Contributions).Where(g => g.UserId == userId);
        if (status.HasValue) query = query.Where(g => g.Status == status.Value);
        return await query.OrderByDescending(g => g.CreatedAt).ToListAsync(cancellationToken);
    }
}
