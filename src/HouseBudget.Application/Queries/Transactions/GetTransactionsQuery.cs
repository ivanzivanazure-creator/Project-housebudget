using HouseBudget.Application.Common;
using HouseBudget.Application.DTOs.Transactions;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Enums;
using MediatR;

namespace HouseBudget.Application.Queries.Transactions;

public record GetTransactionsQuery(DateOnly? From = null, DateOnly? To = null, Guid? CategoryId = null, TransactionType? Type = null, int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<TransactionDto>>;

public sealed class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PaginatedList<TransactionDto>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUser;

    public GetTransactionsQueryHandler(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository, IAccountRepository accountRepository, ICurrentUserService currentUser)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _accountRepository = accountRepository;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByUserIdAsync(_currentUser.UserId, request.From, request.To, request.CategoryId, request.Type, cancellationToken);
        var categories = await _categoryRepository.GetAllForUserAsync(_currentUser.UserId, null, cancellationToken);
        var accounts = await _accountRepository.GetByUserIdAsync(_currentUser.UserId, cancellationToken);

        var dtos = transactions.Select(t =>
        {
            var cat = categories.FirstOrDefault(c => c.Id == t.CategoryId);
            var acc = accounts.FirstOrDefault(a => a.Id == t.AccountId);
            return CreateExpenseCommandHandler.MapToDto(t, acc?.Name ?? "Unknown", cat?.Name ?? "Unknown", cat?.Color ?? "#607D8B");
        }).ToList();

        return PaginatedList<TransactionDto>.Create(dtos, request.PageNumber, request.PageSize);
    }
}
