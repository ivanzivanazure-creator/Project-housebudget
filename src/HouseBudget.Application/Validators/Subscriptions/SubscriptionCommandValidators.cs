using FluentValidation;
using HouseBudget.Application.Commands.Subscriptions;

namespace HouseBudget.Application.Validators.Subscriptions;

public sealed class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
    }
}

public sealed class UpgradeSubscriptionCommandValidator : AbstractValidator<UpgradeSubscriptionCommand>
{
    public UpgradeSubscriptionCommandValidator()
    {
        RuleFor(x => x.NewPlanId).NotEmpty();
    }
}
