using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseBudget.Infrastructure.Repositories;

public sealed class SubscriptionPlanRepository : BaseRepository<SubscriptionPlan>, ISubscriptionPlanRepository
{
    public SubscriptionPlanRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<SubscriptionPlan>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        => await DbSet.Where(p => p.IsActive).OrderBy(p => p.SortOrder).ThenBy(p => p.Price.Amount).ToListAsync(cancellationToken);

    public async Task<SubscriptionPlan?> GetByTierAndPeriodAsync(SubscriptionTier tier, BillingPeriod period, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(p => p.Tier == tier && p.BillingPeriod == period && p.IsActive, cancellationToken);
}
