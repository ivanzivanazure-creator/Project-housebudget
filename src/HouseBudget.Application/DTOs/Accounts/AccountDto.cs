using HouseBudget.Domain.Enums;

namespace HouseBudget.Application.DTOs.Accounts;

public record AccountDto(
    Guid Id,
    string Name,
    AccountType Type,
    string TypeName,
    decimal Balance,
    string Currency,
    string? Description,
    string? BankName,
    string Color,
    string? IconName,
    bool IncludeInNetWorth,
    bool IsDefault,
    DateTime CreatedAt
);
