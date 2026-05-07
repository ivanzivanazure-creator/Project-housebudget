using HouseBudget.Application.DTOs.Transactions;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;
using MediatR;

namespace HouseBudget.Application.Commands.Transactions;

public record CreateExpenseCommand(Guid AccountId, Guid CategoryId, decimal Amount, string Currency, DateOnly TransactionDate, string Description, string? Notes = null, string? Merchant = null, string? Location = null, string[]? Tags = null) : IRequest<TransactionDto>;

public sealed class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, TransactionDto>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateExpenseCommandHandler(ITransactionRepository transactionRepository, IAccountRepository accountRepository, IBudgetRepository budgetRepository, ICategoryRepository categoryRepository, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransactionDto> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(Account), request.AccountId);

        if (account.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        var transaction = Transaction.CreateExpense(_currentUser.UserId, request.AccountId, request.CategoryId, request.Amount, request.Currency, request.TransactionDate, request.Description, request.Notes, request.Merchant, request.Location);

        if (request.Tags?.Any() == true) transaction.SetTags(request.Tags);

        account.Debit(Money.Of(request.Amount, request.Currency));

        var activeBudgets = await _budgetRepository.GetActiveBudgetsForDateAsync(_currentUser.UserId, request.TransactionDate, cancellationToken);
        foreach (var budget in activeBudgets)
        {
            var budgetWithCategories = await _budgetRepository.GetWithCategoriesAsync(budget.Id, cancellationToken);
            if (budgetWithCategories?.BudgetCategories.Any(bc => bc.CategoryId == request.CategoryId) == true)
                budget.RecordSpending(request.CategoryId, Money.Of(request.Amount, request.Currency));
        }

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(transaction, account.Name, category.Name, category.Color);
    }

    public static TransactionDto MapToDto(Transaction t, string accountName, string categoryName, string categoryColor) => new(
        t.Id, t.Type, t.Type.ToString(), t.Amount.Amount, t.Amount.Currency, t.TransactionDate,
        t.Description, t.Notes, t.Merchant, t.Location, t.ReceiptImageUrl,
        t.AccountId, accountName, t.CategoryId, categoryName, categoryColor, t.Tags, t.IsRecurring, t.CreatedAt);
}
