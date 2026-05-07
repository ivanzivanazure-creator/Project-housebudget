using HouseBudget.Application.Commands.Accounts;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBudget.API.Controllers;

/// <summary>Manage financial accounts (bank, cash, credit card, etc.)</summary>
[Authorize]
public sealed class AccountsController : BaseApiController
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUser;

    public AccountsController(IAccountRepository accountRepository, ICurrentUserService currentUser)
    {
        _accountRepository = accountRepository;
        _currentUser = currentUser;
    }

    /// <summary>Get all accounts for the current user</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var accounts = await _accountRepository.GetByUserIdAsync(_currentUser.UserId, ct);
        return Success(accounts.Select(CreateAccountCommandHandler.MapToDto));
    }

    /// <summary>Get account by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Account", id);
        if (account.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();
        return Success(CreateAccountCommandHandler.MapToDto(account));
    }

    /// <summary>Create a new account</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountCommand command, CancellationToken ct)
        => Created(await Mediator.Send(command, ct));

    /// <summary>Update an account</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountRequest request, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Account", id);
        if (account.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();

        account.UpdateDetails(request.Name, request.Description, request.BankName, request.Color, request.IconName, request.IncludeInNetWorth);
        await _accountRepository.UpdateAsync(account, ct);
        return Success(CreateAccountCommandHandler.MapToDto(account));
    }

    /// <summary>Delete an account</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Account", id);
        if (account.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();
        await _accountRepository.DeleteAsync(account, ct);
        return Success<object?>(null, "Account deleted.");
    }
}

public record UpdateAccountRequest(string Name, string? Description, string? BankName, string Color, string? IconName, bool IncludeInNetWorth);
