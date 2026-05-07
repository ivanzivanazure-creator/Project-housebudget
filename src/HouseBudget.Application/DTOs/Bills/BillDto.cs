using HouseBudget.Domain.Enums;

namespace HouseBudget.Application.DTOs.Bills;

public record BillDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Amount,
    string Currency,
    RecurrenceType RecurrenceType,
    string RecurrenceName,
    DateOnly NextDueDate,
    DateOnly? LastPaidDate,
    bool IsDueSoon,
    bool IsOverdue,
    bool AutoPay,
    int ReminderDaysBefore,
    Guid AccountId,
    string AccountName,
    Guid CategoryId,
    string CategoryName,
    string Color,
    bool IsActive,
    DateTime CreatedAt
);
