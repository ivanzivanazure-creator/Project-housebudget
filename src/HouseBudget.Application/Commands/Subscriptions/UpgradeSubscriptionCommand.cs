using HouseBudget.Application.DTOs.Subscriptions;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.Interfaces;
using MediatR;

namespace HouseBudget.Application.Commands.Subscriptions;

public record UpgradeSubscriptionCommand(Guid NewPlanId) : IRequest<SubscriptionDto>;

public sealed class UpgradeSubscriptionCommandHandler : IRequestHandler<UpgradeSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly IPaymentGatewayService _gateway;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpgradeSubscriptionCommandHandler(ISubscriptionRepository subscriptionRepo, ISubscriptionPlanRepository planRepo, IPaymentGatewayService gateway, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _subscriptionRepo = subscriptionRepo;
        _planRepo = planRepo;
        _gateway = gateway;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionDto> Handle(UpgradeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var sub = await _subscriptionRepo.GetActiveByUserIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Subscription), _currentUser.UserId);

        var newPlan = await _planRepo.GetByIdAsync(request.NewPlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(SubscriptionPlan), request.NewPlanId);

        if (!newPlan.IsActive) throw new DomainException("The target plan is not available.");
        if (sub.PlanId == request.NewPlanId) throw new DomainException("Already on this plan.");

        if (!string.IsNullOrEmpty(sub.ExternalSubscriptionId) && !string.IsNullOrEmpty(newPlan.StripePriceId))
            await _gateway.ChangeSubscriptionPlanAsync(sub.ExternalSubscriptionId, newPlan.StripePriceId, cancellationToken);

        sub.Upgrade(request.NewPlanId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SubscriptionMapper.ToDto(sub, newPlan);
    }
}
