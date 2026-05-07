using HouseBudget.Application.DTOs.Reports;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Enums;
using MediatR;

namespace HouseBudget.Application.Queries.Dashboard;

public record GetDashboardQuery : IRequest<DashboardDto>;

public sealed class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IBillRepository _billRepository;
    private readonly IGoalRepository _goalRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;

    public GetDashboardQueryHandler(IAccountRepository accountRepository, ITransactionRepository transactionRepository, IBudgetRepository budgetRepository, IBillRepository billRepository, IGoalRepository goalRepository, ICategoryRepository categoryRepository, ICurrentUserService currentUser)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _budgetRepository = budgetRepository;
        _billRepository = billRepository;
        _goalRepository = goalRepository;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var accounts = await _accountRepository.GetByUserIdAsync(userId, cancellationToken);
        var recentTransactions = await _transactionRepository.GetRecentAsync(userId, 10, cancellationToken);
        var activeBudgets = await _budgetRepository.GetByUserIdAsync(userId, true, cancellationToken);
        var upcomingBills = await _billRepository.GetUpcomingBillsAsync(userId, 7, cancellationToken);
        var activeGoals = await _goalRepository.GetByUserIdAsync(userId, GoalStatus.Active, cancellationToken);
        var categories = await _categoryRepository.GetAllForUserAsync(userId, null, cancellationToken);

        var monthlyIncome = await _transactionRepository.GetTotalByTypeAsync(userId, TransactionType.Income, monthStart, monthEnd, cancellationToken);
        var monthlyExpenses = await _transactionRepository.GetTotalByTypeAsync(userId, TransactionType.Expense, monthStart, monthEnd, cancellationToken);
        var spendingByCategory = await _transactionRepository.GetSpendingByCategoryAsync(userId, monthStart, monthEnd, cancellationToken);

        var netWorthAccounts = accounts.Where(a => a.IncludeInNetWorth);
        var totalBalance = netWorthAccounts.Where(a => a.Type != AccountType.CreditCard && a.Type != AccountType.Loan).Sum(a => a.Balance.Amount);
        var totalDebt = netWorthAccounts.Where(a => a.Type is AccountType.CreditCard or AccountType.Loan).Sum(a => Math.Abs(a.Balance.Amount));
        var totalNetWorth = totalBalance - totalDebt;

        var topCategories = spendingByCategory
            .OrderByDescending(x => x.Value)
            .Take(5)
            .Select(x =>
            {
                var cat = categories.FirstOrDefault(c => c.Id == x.Key);
                return new CategorySpendingDto(x.Key, cat?.Name ?? "Unknown", cat?.Color ?? "#607D8B", x.Value, monthlyExpenses == 0 ? 0 : Math.Round(x.Value / monthlyExpenses * 100, 1));
            }).ToList();

        var currency = accounts.FirstOrDefault()?.Balance.Currency ?? "USD";

        return new DashboardDto(
            totalNetWorth, totalBalance, totalDebt, monthlyIncome, monthlyExpenses,
            monthlyIncome - monthlyExpenses,
            monthlyIncome == 0 ? 0 : Math.Round((monthlyIncome - monthlyExpenses) / monthlyIncome * 100, 1),
            accounts.Select(a => new AccountSummaryDto(a.Id, a.Name, a.Type.ToString(), a.Balance.Amount, a.Color, a.IconName)).ToList(),
            recentTransactions.Select(t => { var cat = categories.FirstOrDefault(c => c.Id == t.CategoryId); return new RecentTransactionDto(t.Id, t.Description, t.Amount.Amount, t.Type.ToString(), cat?.Name ?? "Unknown", cat?.Color ?? "#607D8B", t.TransactionDate); }).ToList(),
            activeBudgets.Select(b => new BudgetSummaryDto(b.Id, b.Name, b.TotalAmount.Amount, b.TotalSpent.Amount, b.UsagePercentage, b.IsOverBudget)).ToList(),
            upcomingBills.Select(b => new UpcomingBillDto(b.Id, b.Name, b.Amount.Amount, b.NextDueDate, b.IsOverdue, b.Color)).ToList(),
            activeGoals.Select(g => new GoalSummaryDto(g.Id, g.Name, g.ProgressPercentage, g.CurrentAmount.Amount, g.TargetAmount.Amount, g.Color)).ToList(),
            topCategories,
            currency
        );
    }
}
