using FluentValidation;
using HouseBudget.Application.Commands.Goals;

namespace HouseBudget.Application.Validators.Goals;

public sealed class CreateGoalCommandValidator : AbstractValidator<CreateGoalCommand>
{
    public CreateGoalCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetAmount).GreaterThan(0).WithMessage("Target amount must be positive.");
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.TargetDate).GreaterThan(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Target date must be in the future.");
    }
}

public sealed class ContributeToGoalCommandValidator : AbstractValidator<ContributeToGoalCommand>
{
    public ContributeToGoalCommandValidator()
    {
        RuleFor(x => x.GoalId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Contribution amount must be positive.");
    }
}
