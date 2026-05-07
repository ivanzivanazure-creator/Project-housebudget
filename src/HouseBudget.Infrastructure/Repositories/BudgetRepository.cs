using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseBudget.Infrastructure.Repositories;

public sealed class BudgetRepository : BaseRepository<Budget>, IBudgetRepository
{
    public BudgetRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Budget>> GetByUserIdAsync(Guid userId, bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = Context.Budgets.Include(b => b.BudgetCategories).Where(b => b.UserId == userId);
        if (activeOnly) query = query.Where(b => b.IsActive);
        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<Budget?> GetWithCategoriesAsync(Guid id, CancellationToken cancellationToken = default)
        => await Context.Budgets.Include(b => b.BudgetCategories).FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Budget>> GetActiveBudgetsForDateAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default)
        => await Context.Budgets.Include(b => b.BudgetCategories)
            .Where(b => b.UserId == userId && b.IsActive && b.Period.Start <= date && b.Period.End >= date)
            .ToListAsync(cancellationToken);
}
