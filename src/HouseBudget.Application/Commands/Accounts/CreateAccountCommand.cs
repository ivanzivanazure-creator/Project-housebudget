using HouseBudget.Application.DTOs.Accounts;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using MediatR;

namespace HouseBudget.Application.Commands.Accounts;

public record CreateAccountCommand(string Name, AccountType Type, decimal InitialBalance, string Currency, string? Description, string? BankName, string Color = "#2196F3", string? IconName = null) : IRequest<AccountDto>;

public sealed class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountCommandHandler(IAccountRepository accountRepository, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = Account.Create(_currentUser.UserId, request.Name, request.Type, request.InitialBalance, request.Currency, request.Description, request.BankName);
        account.UpdateDetails(request.Name, request.Description, request.BankName, request.Color, request.IconName, true);

        var existingAccounts = await _accountRepository.GetByUserIdAsync(_currentUser.UserId, cancellationToken);
        if (!existingAccounts.Any()) account.SetAsDefault();

        await _accountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(account);
    }

    public static AccountDto MapToDto(Account a) => new(
        a.Id, a.Name, a.Type, a.Type.ToString(), a.Balance.Amount, a.Balance.Currency,
        a.Description, a.BankName, a.Color, a.IconName, a.IncludeInNetWorth, a.IsDefault, a.CreatedAt);
}
