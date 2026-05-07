using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseBudget.Infrastructure.Repositories;

public sealed class SubscriptionRepository : BaseRepository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(AppDbContext context) : base(context) { }

    public async Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await Context.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.UserId == userId &&
                (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing || s.Status == SubscriptionStatus.PastDue),
                cancellationToken);

    public async Task<IReadOnlyList<Subscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await Context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Subscription?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
        => await Context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == externalId, cancellationToken);

    public async Task<IReadOnlyList<Subscription>> GetExpiringSoonAsync(int days, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(days);
        return await Context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active &&
                        s.AutoRenew == false &&
                        s.CurrentPeriodEnd <= cutoff)
            .ToListAsync(cancellationToken);
    }
}
