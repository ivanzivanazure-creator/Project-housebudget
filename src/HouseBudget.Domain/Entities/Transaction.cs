using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Events;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;

namespace HouseBudget.Domain.Entities;

public sealed class Transaction : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? ToAccountId { get; private set; }
    public Guid CategoryId { get; private set; }
    public TransactionType Type { get; private set; }
    public Money Amount { get; private set; } = default!;
    public DateOnly TransactionDate { get; private set; }
    public string Description { get; private set; } = default!;
    public string? Notes { get; private set; }
    public string? ReceiptImageUrl { get; private set; }
    public string? Merchant { get; private set; }
    public string? Location { get; private set; }
    public bool IsRecurring { get; private set; }
    public RecurrenceType RecurrenceType { get; private set; }
    public Guid? RecurringBillId { get; private set; }
    public string[] Tags { get; private set; } = Array.Empty<string>();

    public Account Account { get; private set; } = default!;
    public Category Category { get; private set; } = default!;

    private Transaction() { }

    public static Transaction CreateIncome(Guid userId, Guid accountId, Guid categoryId, decimal amount, string currency, DateOnly date, string description, string? notes = null, string? merchant = null)
    {
        ValidateCommon(description, amount);
        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Type = TransactionType.Income,
            Amount = Money.Of(amount, currency),
            TransactionDate = date,
            Description = description.Trim(),
            Notes = notes,
            Merchant = merchant
        };
        transaction.AddDomainEvent(new TransactionCreatedEvent(transaction.Id, userId, amount, TransactionType.Income));
        return transaction;
    }

    public static Transaction CreateExpense(Guid userId, Guid accountId, Guid categoryId, decimal amount, string currency, DateOnly date, string description, string? notes = null, string? merchant = null, string? location = null)
    {
        ValidateCommon(description, amount);
        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Type = TransactionType.Expense,
            Amount = Money.Of(amount, currency),
            TransactionDate = date,
            Description = description.Trim(),
            Notes = notes,
            Merchant = merchant,
            Location = location
        };
        transaction.AddDomainEvent(new TransactionCreatedEvent(transaction.Id, userId, amount, TransactionType.Expense));
        return transaction;
    }

    public static Transaction CreateTransfer(Guid userId, Guid fromAccountId, Guid toAccountId, Guid categoryId, decimal amount, string currency, DateOnly date, string description)
    {
        ValidateCommon(description, amount);
        return new Transaction
        {
            UserId = userId,
            AccountId = fromAccountId,
            ToAccountId = toAccountId,
            CategoryId = categoryId,
            Type = TransactionType.Transfer,
            Amount = Money.Of(amount, currency),
            TransactionDate = date,
            Description = description.Trim()
        };
    }

    public void Update(decimal amount, string currency, DateOnly date, string description, string? notes, Guid categoryId, string? merchant, string? location, string[] tags)
    {
        ValidateCommon(description, amount);
        Amount = Money.Of(amount, currency);
        TransactionDate = date;
        Description = description.Trim();
        Notes = notes;
        CategoryId = categoryId;
        Merchant = merchant;
        Location = location;
        Tags = tags;
        MarkUpdated();
    }

    public void AttachReceipt(string imageUrl) { ReceiptImageUrl = imageUrl; MarkUpdated(); }
    public void SetTags(string[] tags) { Tags = tags; MarkUpdated(); }
    public void LinkToRecurringBill(Guid billId, RecurrenceType recurrence) { RecurringBillId = billId; IsRecurring = true; RecurrenceType = recurrence; MarkUpdated(); }

    private static void ValidateCommon(string description, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Transaction description is required.");
        if (amount <= 0) throw new DomainException("Transaction amount must be positive.");
    }
}
