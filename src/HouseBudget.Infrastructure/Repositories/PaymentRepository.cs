using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseBudget.Infrastructure.Repositories;

public sealed class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Payment>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        => await Context.Payments
            .Include(p => p.Refunds)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Payment>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
        => await Context.Payments
            .Include(p => p.Refunds)
            .Where(p => p.SubscriptionId == subscriptionId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Payment?> GetByExternalIdAsync(string externalPaymentId, CancellationToken cancellationToken = default)
        => await Context.Payments
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId, cancellationToken);

    public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => await Context.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.PaidAt >= from && p.PaidAt <= to)
            .SumAsync(p => p.Amount.Amount, cancellationToken);

    public async Task<int> GetActiveSubscriberCountAsync(CancellationToken cancellationToken = default)
        => await Context.Subscriptions
            .CountAsync(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing, cancellationToken);
}
