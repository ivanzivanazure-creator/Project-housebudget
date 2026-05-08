using FluentValidation;
using HouseBudget.Application.Commands.Accounts;

namespace HouseBudget.Application.Validators.Accounts;

public sealed class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.InitialBalance).GreaterThanOrEqualTo(0);
    }
}
