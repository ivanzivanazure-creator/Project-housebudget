using HouseBudget.Application.DTOs.Payments;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.Interfaces;
using MediatR;

namespace HouseBudget.Application.Commands.Payments;

public record RefundPaymentCommand(Guid PaymentId, decimal Amount, string Reason) : IRequest<PaymentDto>;

public sealed class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, PaymentDto>
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly IPaymentGatewayService _gateway;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RefundPaymentCommandHandler(IPaymentRepository paymentRepo, ISubscriptionPlanRepository planRepo, IPaymentGatewayService gateway, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _paymentRepo = paymentRepo;
        _planRepo = planRepo;
        _gateway = gateway;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentDto> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepo.GetByIdAsync(request.PaymentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Payment), request.PaymentId);

        if (payment.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();
        if (!payment.IsRefundable) throw new DomainException("This payment is not refundable.");

        // Issue refund in gateway
        if (!string.IsNullOrEmpty(payment.ExternalPaymentId))
        {
            var externalRefundId = await _gateway.RefundPaymentAsync(
                payment.ExternalPaymentId, request.Amount, request.Reason, cancellationToken);

            var refund = payment.Refund(request.Amount, request.Reason);
            if (!string.IsNullOrEmpty(externalRefundId)) refund.SetExternalRefundId(externalRefundId);
        }
        else
        {
            payment.Refund(request.Amount, request.Reason);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var plan = await _planRepo.GetByIdAsync(payment.PlanId, cancellationToken);
        return PaymentMapper.ToDto(payment, plan?.Name ?? "Unknown");
    }
}
