using HouseBudget.Application.DTOs.Subscriptions;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Exceptions;
using MediatR;

namespace HouseBudget.Application.Queries.Subscriptions;

public record GetMySubscriptionQuery : IRequest<SubscriptionDto?>;

public sealed class GetMySubscriptionQueryHandler : IRequestHandler<GetMySubscriptionQuery, SubscriptionDto?>
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly ICurrentUserService _currentUser;

    public GetMySubscriptionQueryHandler(ISubscriptionRepository subscriptionRepo, ISubscriptionPlanRepository planRepo, ICurrentUserService currentUser)
    {
        _subscriptionRepo = subscriptionRepo;
        _planRepo = planRepo;
        _currentUser = currentUser;
    }

    public async Task<SubscriptionDto?> Handle(GetMySubscriptionQuery request, CancellationToken cancellationToken)
    {
        var sub = await _subscriptionRepo.GetActiveByUserIdAsync(_currentUser.UserId, cancellationToken);
        if (sub is null) return null;

        var plan = await _planRepo.GetByIdAsync(sub.PlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(SubscriptionPlan), sub.PlanId);

        return SubscriptionMapper.ToDto(sub, plan);
    }
}

public record GetSubscriptionHistoryQuery : IRequest<IReadOnlyList<SubscriptionDto>>;

public sealed class GetSubscriptionHistoryQueryHandler : IRequestHandler<GetSubscriptionHistoryQuery, IReadOnlyList<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly ICurrentUserService _currentUser;

    public GetSubscriptionHistoryQueryHandler(ISubscriptionRepository subscriptionRepo, ISubscriptionPlanRepository planRepo, ICurrentUserService currentUser)
    {
        _subscriptionRepo = subscriptionRepo;
        _planRepo = planRepo;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SubscriptionDto>> Handle(GetSubscriptionHistoryQuery request, CancellationToken cancellationToken)
    {
        var subs = await _subscriptionRepo.GetByUserIdAsync(_currentUser.UserId, cancellationToken);
        var result = new List<SubscriptionDto>();
        foreach (var sub in subs)
        {
            var plan = await _planRepo.GetByIdAsync(sub.PlanId, cancellationToken);
            if (plan is not null) result.Add(SubscriptionMapper.ToDto(sub, plan));
        }
        return result;
    }
}
