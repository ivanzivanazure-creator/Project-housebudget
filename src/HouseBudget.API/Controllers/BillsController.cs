using HouseBudget.Application.Commands.Bills;
using HouseBudget.Application.DTOs.Bills;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBudget.API.Controllers;

/// <summary>Manage recurring bills and reminders</summary>
[Authorize]
public sealed class BillsController : BaseApiController
{
    private readonly IBillRepository _billRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;

    public BillsController(IBillRepository billRepository, IAccountRepository accountRepository, ICategoryRepository categoryRepository, ICurrentUserService currentUser)
    {
        _billRepository = billRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
    }

    /// <summary>Get all bills for current user</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
    {
        var bills = await _billRepository.GetByUserIdAsync(_currentUser.UserId, activeOnly, ct);
        return Success(await MapBillsAsync(bills, ct));
    }

    /// <summary>Get upcoming bills (within next 7 days)</summary>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming([FromQuery] int days = 7, CancellationToken ct = default)
    {
        var bills = await _billRepository.GetUpcomingBillsAsync(_currentUser.UserId, days, ct);
        return Success(await MapBillsAsync(bills, ct));
    }

    /// <summary>Get overdue bills</summary>
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue(CancellationToken ct)
    {
        var bills = await _billRepository.GetOverdueBillsAsync(_currentUser.UserId, ct);
        return Success(await MapBillsAsync(bills, ct));
    }

    /// <summary>Create a new recurring bill</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBillCommand command, CancellationToken ct)
        => Created(await Mediator.Send(command, ct));

    /// <summary>Mark a bill as paid</summary>
    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> MarkAsPaid(Guid id, [FromBody] MarkPaidRequest request, CancellationToken ct)
    {
        var bill = await _billRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Bill", id);
        if (bill.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();
        bill.MarkAsPaid(request.PaidDate);
        await _billRepository.UpdateAsync(bill, ct);
        return Success<object?>(null, "Bill marked as paid.");
    }

    private async Task<IEnumerable<BillDto>> MapBillsAsync(IEnumerable<Domain.Entities.Bill> bills, CancellationToken ct)
    {
        var accounts = await _accountRepository.GetByUserIdAsync(_currentUser.UserId, ct);
        var categories = await _categoryRepository.GetAllForUserAsync(_currentUser.UserId, null, ct);
        return bills.Select(b =>
        {
            var acc = accounts.FirstOrDefault(a => a.Id == b.AccountId);
            var cat = categories.FirstOrDefault(c => c.Id == b.CategoryId);
            return new BillDto(b.Id, b.Name, b.Description, b.Amount.Amount, b.Amount.Currency, b.RecurrenceType, b.RecurrenceType.ToString(), b.NextDueDate, b.LastPaidDate, b.IsDueSoon, b.IsOverdue, b.AutoPay, b.ReminderDaysBefore, b.AccountId, acc?.Name ?? "Unknown", b.CategoryId, cat?.Name ?? "Unknown", b.Color, b.IsActive, b.CreatedAt);
        });
    }
}

public record MarkPaidRequest(DateOnly PaidDate);
