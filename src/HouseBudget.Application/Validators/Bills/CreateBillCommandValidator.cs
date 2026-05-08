using FluentValidation;
using HouseBudget.Application.Commands.Bills;

namespace HouseBudget.Application.Validators.Bills;

public sealed class CreateBillCommandValidator : AbstractValidator<CreateBillCommand>
{
    public CreateBillCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be positive.");
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.NextDueDate).NotEmpty();
        RuleFor(x => x.ReminderDaysBefore).InclusiveBetween(0, 30);
    }
}
