using HouseBudget.Domain.Enums;

namespace HouseBudget.Application.DTOs.Budgets;

public record BudgetDto(
    Guid Id,
    string Name,
    string? Description,
    BudgetPeriodType PeriodType,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal TotalAmount,
    decimal TotalAllocated,
    decimal TotalSpent,
    decimal Remaining,
    decimal UsagePercentage,
    bool IsOverBudget,
    string Currency,
    bool IsActive,
    List<BudgetCategoryDto> Categories,
    DateTime CreatedAt
);

public record BudgetCategoryDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string CategoryColor,
    decimal AllocatedAmount,
    decimal SpentAmount,
    decimal Remaining,
    decimal UsagePercentage,
    bool IsOverBudget
);
