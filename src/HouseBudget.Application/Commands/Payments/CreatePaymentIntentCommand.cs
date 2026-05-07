using HouseBudget.Application.DTOs.Payments;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Exceptions;
using MediatR;

namespace HouseBudget.Application.Commands.Payments;

public record CreatePaymentIntentCommand(Guid PlanId) : IRequest<PaymentIntentDto>;

public sealed class CreatePaymentIntentCommandHandler : IRequestHandler<CreatePaymentIntentCommand, PaymentIntentDto>
{
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly IUserRepository _userRepo;
    private readonly IPaymentGatewayService _gateway;
    private readonly ICurrentUserService _currentUser;

    public CreatePaymentIntentCommandHandler(ISubscriptionPlanRepository planRepo, ISubscriptionRepository subscriptionRepo, IUserRepository userRepo, IPaymentGatewayService gateway, ICurrentUserService currentUser)
    {
        _planRepo = planRepo;
        _subscriptionRepo = subscriptionRepo;
        _userRepo = userRepo;
        _gateway = gateway;
        _currentUser = currentUser;
    }

    public async Task<PaymentIntentDto> Handle(CreatePaymentIntentCommand request, CancellationToken cancellationToken)
    {
        var plan = await _planRepo.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(SubscriptionPlan), request.PlanId);

        var user = await _userRepo.GetByIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), _currentUser.UserId);

        var sub = await _subscriptionRepo.GetActiveByUserIdAsync(_currentUser.UserId, cancellationToken);
        var customerId = sub?.ExternalCustomerId
            ?? await _gateway.CreateCustomerAsync(user.Email.Value, user.FullName, cancellationToken);

        var result = await _gateway.CreatePaymentIntentAsync(
            plan.Price.Amount, plan.Price.Currency, customerId,
            $"HouseBudget {plan.Name} subscription", cancellationToken);

        return new PaymentIntentDto(result.ClientSecret, result.PaymentIntentId, plan.Price.Amount, plan.Price.Currency);
    }
}
