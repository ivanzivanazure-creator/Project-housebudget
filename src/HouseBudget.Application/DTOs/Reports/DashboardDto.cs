namespace HouseBudget.Application.DTOs.Reports;

public record DashboardDto(
    decimal TotalNetWorth,
    decimal TotalBalance,
    decimal TotalDebt,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    decimal MonthlySavings,
    decimal SavingsRate,
    List<AccountSummaryDto> Accounts,
    List<RecentTransactionDto> RecentTransactions,
    List<BudgetSummaryDto> ActiveBudgets,
    List<UpcomingBillDto> UpcomingBills,
    List<GoalSummaryDto> ActiveGoals,
    List<CategorySpendingDto> TopExpenseCategories,
    string Currency
);

public record AccountSummaryDto(Guid Id, string Name, string Type, decimal Balance, string Color, string? IconName);
public record RecentTransactionDto(Guid Id, string Description, decimal Amount, string Type, string Category, string CategoryColor, DateOnly Date);
public record BudgetSummaryDto(Guid Id, string Name, decimal TotalAmount, decimal SpentAmount, decimal UsagePercentage, bool IsOverBudget);
public record UpcomingBillDto(Guid Id, string Name, decimal Amount, DateOnly DueDate, bool IsOverdue, string Color);
public record GoalSummaryDto(Guid Id, string Name, decimal ProgressPercentage, decimal CurrentAmount, decimal TargetAmount, string Color);
public record CategorySpendingDto(Guid CategoryId, string CategoryName, string Color, decimal Amount, decimal Percentage);
