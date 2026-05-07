namespace HouseBudget.Application.DTOs.Reports;

public record MonthlyReportDto(
    int Year,
    int Month,
    string MonthName,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetSavings,
    decimal SavingsRate,
    List<CategorySpendingDto> ExpensesByCategory,
    List<CategorySpendingDto> IncomeByCategory,
    List<DailySpendingDto> DailySpending,
    string Currency
);

public record DailySpendingDto(DateOnly Date, decimal Income, decimal Expenses);

public record AnnualReportDto(
    int Year,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetSavings,
    decimal AverageMonthlySavings,
    List<MonthlyBreakdownDto> MonthlyBreakdown,
    List<CategorySpendingDto> TopExpenseCategories,
    string Currency
);

public record MonthlyBreakdownDto(int Month, string MonthName, decimal Income, decimal Expenses, decimal Net);
