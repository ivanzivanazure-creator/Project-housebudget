using HouseBudget.Application.Commands.Transactions;
using HouseBudget.Application.Interfaces;
using HouseBudget.Application.Queries.Transactions;
using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBudget.API.Controllers;

/// <summary>Manage income and expense transactions</summary>
[Authorize]
public sealed class TransactionsController : BaseApiController
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUser;

    public TransactionsController(ITransactionRepository transactionRepository, ICurrentUserService currentUser)
    {
        _transactionRepository = transactionRepository;
        _currentUser = currentUser;
    }

    /// <summary>Get paginated transactions with filters</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] Guid? categoryId, [FromQuery] TransactionType? type, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Success(await Mediator.Send(new GetTransactionsQuery(from, to, categoryId, type, page, pageSize), ct));

    /// <summary>Get transaction by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var t = await _transactionRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Transaction", id);
        if (t.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();
        return Success(t);
    }

    /// <summary>Create an expense transaction</summary>
    [HttpPost("expenses")]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseCommand command, CancellationToken ct)
        => Created(await Mediator.Send(command, ct));

    /// <summary>Create an income transaction</summary>
    [HttpPost("income")]
    public async Task<IActionResult> CreateIncome([FromBody] CreateIncomeCommand command, CancellationToken ct)
        => Created(await Mediator.Send(command, ct));

    /// <summary>Delete a transaction</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var t = await _transactionRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Transaction", id);
        if (t.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();
        await _transactionRepository.DeleteAsync(t, ct);
        return Success<object?>(null, "Transaction deleted.");
    }
}
