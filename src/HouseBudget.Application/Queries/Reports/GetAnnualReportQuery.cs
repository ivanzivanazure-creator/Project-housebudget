using HouseBudget.Application.DTOs.Reports;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Enums;
using MediatR;

namespace HouseBudget.Application.Queries.Reports;

public record GetAnnualReportQuery(int Year) : IRequest<AnnualReportDto>;

public sealed class GetAnnualReportQueryHandler : IRequestHandler<GetAnnualReportQuery, AnnualReportDto>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;

    public GetAnnualReportQueryHandler(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository, ICurrentUserService currentUser)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
    }

    public async Task<AnnualReportDto> Handle(GetAnnualReportQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var yearStart = new DateOnly(request.Year, 1, 1);
        var yearEnd = new DateOnly(request.Year, 12, 31);

        var transactions = await _transactionRepository.GetByUserIdAsync(userId, yearStart, yearEnd, null, null, cancellationToken);
        var categories = await _categoryRepository.GetAllForUserAsync(userId, null, cancellationToken);

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount.Amount);
        var totalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount.Amount);

        var monthlyBreakdown = Enumerable.Range(1, 12).Select(month =>
        {
            var monthTx = transactions.Where(t => t.TransactionDate.Month == month);
            var income = monthTx.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount.Amount);
            var expenses = monthTx.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount.Amount);
            return new MonthlyBreakdownDto(month, new DateTime(request.Year, month, 1).ToString("MMM"), income, expenses, income - expenses);
        }).ToList();

        var topExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .Select(g =>
            {
                var cat = categories.FirstOrDefault(c => c.Id == g.Key);
                var amount = g.Sum(t => t.Amount.Amount);
                return new CategorySpendingDto(g.Key, cat?.Name ?? "Unknown", cat?.Color ?? "#607D8B", amount, totalExpenses == 0 ? 0 : Math.Round(amount / totalExpenses * 100, 1));
            })
            .OrderByDescending(x => x.Amount)
            .Take(10)
            .ToList();

        var currency = transactions.FirstOrDefault()?.Amount.Currency ?? "USD";

        return new AnnualReportDto(request.Year, totalIncome, totalExpenses, totalIncome - totalExpenses,
            monthlyBreakdown.Average(m => m.Income - m.Expenses), monthlyBreakdown, topExpenses, currency);
    }
}
