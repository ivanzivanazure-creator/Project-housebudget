using FluentValidation;
using HouseBudget.Application.Commands.Transactions;

namespace HouseBudget.Application.Validators.Transactions;

public sealed class CreateIncomeCommandValidator : AbstractValidator<CreateIncomeCommand>
{
    public CreateIncomeCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be positive.");
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TransactionDate).NotEmpty();
    }
}
