using HouseBudget.Application.DTOs.Reports;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Enums;
using MediatR;

namespace HouseBudget.Application.Queries.Reports;

public record GetMonthlyReportQuery(int Year, int Month) : IRequest<MonthlyReportDto>;

public sealed class GetMonthlyReportQueryHandler : IRequestHandler<GetMonthlyReportQuery, MonthlyReportDto>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;

    public GetMonthlyReportQueryHandler(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository, ICurrentUserService currentUser)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
    }

    public async Task<MonthlyReportDto> Handle(GetMonthlyReportQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var start = new DateOnly(request.Year, request.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var transactions = await _transactionRepository.GetByUserIdAsync(userId, start, end, null, null, cancellationToken);
        var categories = await _categoryRepository.GetAllForUserAsync(userId, null, cancellationToken);

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount.Amount);
        var totalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount.Amount);

        var expensesByCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .Select(g =>
            {
                var cat = categories.FirstOrDefault(c => c.Id == g.Key);
                var amount = g.Sum(t => t.Amount.Amount);
                return new CategorySpendingDto(g.Key, cat?.Name ?? "Unknown", cat?.Color ?? "#607D8B", amount, totalExpenses == 0 ? 0 : Math.Round(amount / totalExpenses * 100, 1));
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        var incomeByCategory = transactions
            .Where(t => t.Type == TransactionType.Income)
            .GroupBy(t => t.CategoryId)
            .Select(g =>
            {
                var cat = categories.FirstOrDefault(c => c.Id == g.Key);
                var amount = g.Sum(t => t.Amount.Amount);
                return new CategorySpendingDto(g.Key, cat?.Name ?? "Unknown", cat?.Color ?? "#607D8B", amount, totalIncome == 0 ? 0 : Math.Round(amount / totalIncome * 100, 1));
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        var dailySpending = Enumerable.Range(1, end.Day)
            .Select(day =>
            {
                var date = new DateOnly(request.Year, request.Month, day);
                var dayTx = transactions.Where(t => t.TransactionDate == date);
                return new DailySpendingDto(date, dayTx.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount.Amount), dayTx.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount.Amount));
            }).ToList();

        var currency = transactions.FirstOrDefault()?.Amount.Currency ?? "USD";

        return new MonthlyReportDto(request.Year, request.Month,
            new DateTime(request.Year, request.Month, 1).ToString("MMMM"),
            totalIncome, totalExpenses, totalIncome - totalExpenses,
            totalIncome == 0 ? 0 : Math.Round((totalIncome - totalExpenses) / totalIncome * 100, 1),
            expensesByCategory, incomeByCategory, dailySpending, currency);
    }
}
