using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseBudget.Infrastructure.Repositories;

public sealed class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Transaction>> GetByUserIdAsync(Guid userId, DateOnly? from = null, DateOnly? to = null, Guid? categoryId = null, TransactionType? type = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(t => t.UserId == userId);
        if (from.HasValue) query = query.Where(t => t.TransactionDate >= from.Value);
        if (to.HasValue) query = query.Where(t => t.TransactionDate <= to.Value);
        if (categoryId.HasValue) query = query.Where(t => t.CategoryId == categoryId.Value);
        if (type.HasValue) query = query.Where(t => t.Type == type.Value);
        return await query.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
        => await DbSet.Where(t => t.AccountId == accountId).OrderByDescending(t => t.TransactionDate).ToListAsync(cancellationToken);

    public async Task<decimal> GetTotalByTypeAsync(Guid userId, TransactionType type, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => await DbSet.Where(t => t.UserId == userId && t.Type == type && t.TransactionDate >= from && t.TransactionDate <= to).SumAsync(t => t.Amount.Amount, cancellationToken);

    public async Task<Dictionary<Guid, decimal>> GetSpendingByCategoryAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.TransactionDate >= from && t.TransactionDate <= to)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(t => t.Amount.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Total, cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetRecentAsync(Guid userId, int count, CancellationToken cancellationToken = default)
        => await DbSet.Where(t => t.UserId == userId).OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.CreatedAt).Take(count).ToListAsync(cancellationToken);
}
