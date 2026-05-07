namespace HouseBudget.Mobile.Models;

public record DashboardData(
    decimal TotalNetWorth,
    decimal TotalBalance,
    decimal TotalDebt,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    decimal MonthlySavings,
    decimal SavingsRate,
    List<AccountSummary> Accounts,
    List<RecentTransaction> RecentTransactions,
    List<BudgetSummary> ActiveBudgets,
    List<UpcomingBill> UpcomingBills,
    List<GoalSummary> ActiveGoals,
    List<CategorySpending> TopExpenseCategories,
    string Currency
);

public record AccountSummary(Guid Id, string Name, string Type, decimal Balance, string Color, string? IconName);
public record RecentTransaction(Guid Id, string Description, decimal Amount, string Type, string Category, string CategoryColor, DateOnly Date);
public record BudgetSummary(Guid Id, string Name, decimal TotalAmount, decimal SpentAmount, decimal UsagePercentage, bool IsOverBudget);
public record UpcomingBill(Guid Id, string Name, decimal Amount, DateOnly DueDate, bool IsOverdue, string Color);
public record GoalSummary(Guid Id, string Name, decimal ProgressPercentage, decimal CurrentAmount, decimal TargetAmount, string Color);
public record CategorySpending(Guid CategoryId, string CategoryName, string Color, decimal Amount, decimal Percentage);

public record TransactionItem(Guid Id, string TypeName, decimal Amount, string Currency, DateOnly TransactionDate, string Description, string AccountName, string CategoryName, string CategoryColor);
public record BudgetItem(Guid Id, string Name, decimal TotalAmount, decimal TotalSpent, decimal UsagePercentage, bool IsOverBudget, string Currency, bool IsActive);
public record GoalItem(Guid Id, string Name, decimal TargetAmount, decimal CurrentAmount, decimal ProgressPercentage, string Currency, string StatusName, string Color, decimal? MonthlyRequired);
public record BillItem(Guid Id, string Name, decimal Amount, string Currency, string RecurrenceName, DateOnly NextDueDate, bool IsDueSoon, bool IsOverdue, string Color, bool IsActive);

public record CategoryModel(Guid Id, string Name, string TypeName, string Color, bool IsSystem);

public record CategoryBreakdownItem(string CategoryName, string Color, decimal Amount, decimal Percentage);

public record MonthlyTrendItem(string Month, decimal Income, decimal Expenses)
{
    public double IncomeBarWidth => (double)Math.Min(Income / 10m, 200m);
    public double ExpensesBarWidth => (double)Math.Min(Expenses / 10m, 200m);
}

public record MonthlyReportData(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetSavings,
    decimal SavingsRate,
    List<CategoryBreakdownItem> CategoryBreakdown,
    List<MonthlyTrendItem> MonthlyTrend
);

public record AnnualReportData(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetSavings,
    decimal SavingsRate,
    List<CategoryBreakdownItem> CategoryBreakdown,
    List<MonthlyTrendItem> MonthlyTrend
);
