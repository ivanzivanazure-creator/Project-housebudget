using FluentValidation;
using HouseBudget.Application.Commands.Budgets;

namespace HouseBudget.Application.Validators.Budgets;

public sealed class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TotalAmount).GreaterThan(0).WithMessage("Budget amount must be positive.");
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");
        RuleFor(x => x.Categories).NotEmpty().WithMessage("At least one category allocation is required.");
        RuleForEach(x => x.Categories).ChildRules(cat =>
        {
            cat.RuleFor(c => c.CategoryId).NotEmpty();
            cat.RuleFor(c => c.AllocatedAmount).GreaterThan(0);
        });
    }
}
