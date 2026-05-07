using HouseBudget.Domain.Enums;

namespace HouseBudget.Application.DTOs.Goals;

public record GoalDto(
    Guid Id,
    string Name,
    string? Description,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal RemainingAmount,
    decimal ProgressPercentage,
    string Currency,
    DateOnly? TargetDate,
    GoalStatus Status,
    string StatusName,
    string Color,
    string? IconName,
    decimal? MonthlyRequired,
    DateTime CreatedAt
);
