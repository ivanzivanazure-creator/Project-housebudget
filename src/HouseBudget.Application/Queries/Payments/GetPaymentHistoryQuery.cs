using HouseBudget.Application.Common;
using HouseBudget.Application.DTOs.Payments;
using HouseBudget.Application.Interfaces;
using MediatR;

namespace HouseBudget.Application.Queries.Payments;

public record GetPaymentHistoryQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<PaymentDto>>;

public sealed class GetPaymentHistoryQueryHandler : IRequestHandler<GetPaymentHistoryQuery, PaginatedList<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly ICurrentUserService _currentUser;

    public GetPaymentHistoryQueryHandler(IPaymentRepository paymentRepo, ISubscriptionPlanRepository planRepo, ICurrentUserService currentUser)
    {
        _paymentRepo = paymentRepo;
        _planRepo = planRepo;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<PaymentDto>> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        var payments = await _paymentRepo.GetByUserIdAsync(_currentUser.UserId, request.PageNumber, request.PageSize, cancellationToken);
        var dtos = new List<PaymentDto>();

        foreach (var p in payments)
        {
            var plan = await _planRepo.GetByIdAsync(p.PlanId, cancellationToken);
            dtos.Add(PaymentMapper.ToDto(p, plan?.Name ?? "Unknown"));
        }

        var total = await _paymentRepo.CountAsync(p => p.UserId == _currentUser.UserId, cancellationToken);
        return new PaginatedList<PaymentDto>(dtos, total, request.PageNumber, request.PageSize);
    }
}
