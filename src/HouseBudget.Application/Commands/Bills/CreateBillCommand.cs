using HouseBudget.Application.DTOs.Bills;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Exceptions;
using MediatR;

namespace HouseBudget.Application.Commands.Bills;

public record CreateBillCommand(string Name, decimal Amount, string Currency, RecurrenceType RecurrenceType, DateOnly NextDueDate, Guid AccountId, Guid CategoryId, string? Description = null, bool AutoPay = false, int ReminderDaysBefore = 3, string Color = "#FF5722") : IRequest<BillDto>;

public sealed class CreateBillCommandHandler : IRequestHandler<CreateBillCommand, BillDto>
{
    private readonly IBillRepository _billRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBillCommandHandler(IBillRepository billRepository, IAccountRepository accountRepository, ICategoryRepository categoryRepository, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _billRepository = billRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<BillDto> Handle(CreateBillCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(Account), request.AccountId);
        if (account.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        var bill = Bill.Create(_currentUser.UserId, request.AccountId, request.CategoryId, request.Name, request.Amount, request.Currency, request.RecurrenceType, request.NextDueDate, request.Description, request.AutoPay, request.ReminderDaysBefore);

        await _billRepository.AddAsync(bill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BillDto(bill.Id, bill.Name, bill.Description, bill.Amount.Amount, bill.Amount.Currency, bill.RecurrenceType, bill.RecurrenceType.ToString(), bill.NextDueDate, bill.LastPaidDate, bill.IsDueSoon, bill.IsOverdue, bill.AutoPay, bill.ReminderDaysBefore, request.AccountId, account.Name, request.CategoryId, category.Name, bill.Color, bill.IsActive, bill.CreatedAt);
    }
}
