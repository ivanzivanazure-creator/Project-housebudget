using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Application.Interfaces;

public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
{
    Task<IReadOnlyList<SubscriptionPlan>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetByTierAndPeriodAsync(SubscriptionTier tier, BillingPeriod period, CancellationToken cancellationToken = default);
}
