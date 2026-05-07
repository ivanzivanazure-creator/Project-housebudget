using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseBudget.Infrastructure.Repositories;

public sealed class BillRepository : BaseRepository<Bill>, IBillRepository
{
    public BillRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Bill>> GetByUserIdAsync(Guid userId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(b => b.UserId == userId);
        if (activeOnly) query = query.Where(b => b.IsActive);
        return await query.OrderBy(b => b.NextDueDate).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetUpcomingBillsAsync(Guid userId, int days = 7, CancellationToken cancellationToken = default)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.Today.AddDays(days));
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await DbSet.Where(b => b.UserId == userId && b.IsActive && b.NextDueDate >= today && b.NextDueDate <= cutoff).OrderBy(b => b.NextDueDate).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetOverdueBillsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await DbSet.Where(b => b.UserId == userId && b.IsActive && b.NextDueDate < today).OrderBy(b => b.NextDueDate).ToListAsync(cancellationToken);
    }
}
