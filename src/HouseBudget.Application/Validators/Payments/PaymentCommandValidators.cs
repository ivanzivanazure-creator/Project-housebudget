using FluentValidation;
using HouseBudget.Application.Commands.Payments;

namespace HouseBudget.Application.Validators.Payments;

public sealed class CreatePaymentIntentCommandValidator : AbstractValidator<CreatePaymentIntentCommand>
{
    public CreatePaymentIntentCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be positive.");
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}

public sealed class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Refund amount must be positive.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
