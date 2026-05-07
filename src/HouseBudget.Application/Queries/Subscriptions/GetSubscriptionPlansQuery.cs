using HouseBudget.Application.DTOs.Subscriptions;
using HouseBudget.Application.Interfaces;
using MediatR;

namespace HouseBudget.Application.Queries.Subscriptions;

public record GetSubscriptionPlansQuery : IRequest<IReadOnlyList<SubscriptionPlanDto>>;

public sealed class GetSubscriptionPlansQueryHandler : IRequestHandler<GetSubscriptionPlansQuery, IReadOnlyList<SubscriptionPlanDto>>
{
    private readonly ISubscriptionPlanRepository _planRepo;

    public GetSubscriptionPlansQueryHandler(ISubscriptionPlanRepository planRepo) => _planRepo = planRepo;

    public async Task<IReadOnlyList<SubscriptionPlanDto>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _planRepo.GetAllActiveAsync(cancellationToken);
        return plans.Select(SubscriptionMapper.ToPlanDto).ToList();
    }
}
