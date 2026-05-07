using HouseBudget.Application.DTOs.Subscriptions;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.Interfaces;
using MediatR;

namespace HouseBudget.Application.Commands.Subscriptions;

public record CreateSubscriptionCommand(Guid PlanId, BillingPeriod BillingPeriod, bool UseFreeTrial = true) : IRequest<SubscriptionDto>;

public sealed class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly IUserRepository _userRepo;
    private readonly IPaymentGatewayService _gateway;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSubscriptionCommandHandler(ISubscriptionRepository subscriptionRepo, ISubscriptionPlanRepository planRepo, IUserRepository userRepo, IPaymentGatewayService gateway, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _subscriptionRepo = subscriptionRepo;
        _planRepo = planRepo;
        _userRepo = userRepo;
        _gateway = gateway;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var existing = await _subscriptionRepo.GetActiveByUserIdAsync(_currentUser.UserId, cancellationToken);
        if (existing is not null) throw new DomainException("User already has an active subscription. Upgrade or cancel first.");

        var plan = await _planRepo.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(SubscriptionPlan), request.PlanId);

        if (!plan.IsActive) throw new DomainException("This plan is no longer available.");

        var user = await _userRepo.GetByIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), _currentUser.UserId);

        // Free tier — no payment gateway needed
        if (plan.Tier == SubscriptionTier.Free)
        {
            var freeSub = Subscription.Create(_currentUser.UserId, plan.Id, request.BillingPeriod, 0);
            freeSub.Activate();
            await _subscriptionRepo.AddAsync(freeSub, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await MapToDtoAsync(freeSub, plan);
        }

        // Paid tier — create gateway customer & subscription
        var customerId = await _gateway.CreateCustomerAsync(user.Email.Value, user.FullName, cancellationToken);
        var trialDays = request.UseFreeTrial && plan.TrialDays > 0 ? plan.TrialDays : 0;

        var gatewayResult = await _gateway.CreateSubscriptionAsync(
            customerId, plan.StripePriceId ?? plan.Id.ToString(), trialDays > 0, trialDays, cancellationToken);

        var subscription = Subscription.Create(_currentUser.UserId, plan.Id, request.BillingPeriod, trialDays);
        subscription.SetExternalIds(gatewayResult.SubscriptionId, customerId);

        if (!gatewayResult.IsTrialing) subscription.Activate(gatewayResult.SubscriptionId, customerId);

        await _subscriptionRepo.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapToDtoAsync(subscription, plan);
    }

    private static Task<SubscriptionDto> MapToDtoAsync(Subscription s, SubscriptionPlan plan) =>
        Task.FromResult(SubscriptionMapper.ToDto(s, plan));
}
