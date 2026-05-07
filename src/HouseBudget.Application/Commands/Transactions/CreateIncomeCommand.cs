using HouseBudget.Application.DTOs.Transactions;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;
using MediatR;

namespace HouseBudget.Application.Commands.Transactions;

public record CreateIncomeCommand(Guid AccountId, Guid CategoryId, decimal Amount, string Currency, DateOnly TransactionDate, string Description, string? Notes = null, string? Merchant = null) : IRequest<TransactionDto>;

public sealed class CreateIncomeCommandHandler : IRequestHandler<CreateIncomeCommand, TransactionDto>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateIncomeCommandHandler(ITransactionRepository transactionRepository, IAccountRepository accountRepository, ICategoryRepository categoryRepository, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransactionDto> Handle(CreateIncomeCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(Account), request.AccountId);
        if (account.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        var transaction = Transaction.CreateIncome(_currentUser.UserId, request.AccountId, request.CategoryId, request.Amount, request.Currency, request.TransactionDate, request.Description, request.Notes, request.Merchant);
        account.Credit(Money.Of(request.Amount, request.Currency));

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateExpenseCommandHandler.MapToDto(transaction, account.Name, category.Name, category.Color);
    }
}
