using HouseBudget.Application.DTOs.Subscriptions;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.Interfaces;
using MediatR;

namespace HouseBudget.Application.Commands.Subscriptions;

public record CancelSubscriptionCommand(string? Reason = null, bool AtPeriodEnd = true) : IRequest<SubscriptionDto>;

public sealed class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly IPaymentGatewayService _gateway;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CancelSubscriptionCommandHandler(ISubscriptionRepository subscriptionRepo, ISubscriptionPlanRepository planRepo, IPaymentGatewayService gateway, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _subscriptionRepo = subscriptionRepo;
        _planRepo = planRepo;
        _gateway = gateway;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionDto> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var sub = await _subscriptionRepo.GetActiveByUserIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Subscription), _currentUser.UserId);

        // Cancel in gateway if linked
        if (!string.IsNullOrEmpty(sub.ExternalSubscriptionId))
            await _gateway.CancelSubscriptionAsync(sub.ExternalSubscriptionId, request.AtPeriodEnd, cancellationToken);

        sub.Cancel(request.Reason, request.AtPeriodEnd);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var plan = await _planRepo.GetByIdAsync(sub.PlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(SubscriptionPlan), sub.PlanId);

        return SubscriptionMapper.ToDto(sub, plan);
    }
}
