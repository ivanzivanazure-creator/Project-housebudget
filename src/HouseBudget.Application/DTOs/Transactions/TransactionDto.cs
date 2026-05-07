using HouseBudget.Domain.Enums;

namespace HouseBudget.Application.DTOs.Transactions;

public record TransactionDto(
    Guid Id,
    TransactionType Type,
    string TypeName,
    decimal Amount,
    string Currency,
    DateOnly TransactionDate,
    string Description,
    string? Notes,
    string? Merchant,
    string? Location,
    string? ReceiptImageUrl,
    Guid AccountId,
    string AccountName,
    Guid CategoryId,
    string CategoryName,
    string CategoryColor,
    string[] Tags,
    bool IsRecurring,
    DateTime CreatedAt
);
