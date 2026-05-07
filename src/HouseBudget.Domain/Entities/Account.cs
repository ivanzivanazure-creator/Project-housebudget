using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;

namespace HouseBudget.Domain.Entities;

public sealed class Account : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = default!;
    public AccountType Type { get; private set; }
    public Money Balance { get; private set; } = default!;
    public Money InitialBalance { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? BankName { get; private set; }
    public string? AccountNumber { get; private set; }
    public string Color { get; private set; } = "#2196F3";
    public string? IconName { get; private set; }
    public bool IncludeInNetWorth { get; private set; } = true;
    public bool IsDefault { get; private set; }

    public User User { get; private set; } = default!;

    private Account() { }

    public static Account Create(Guid userId, string name, AccountType type, decimal initialBalance, string currency = "USD", string? description = null, string? bankName = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Account name is required.");

        var balance = Money.Of(initialBalance, currency);
        return new Account
        {
            UserId = userId,
            Name = name.Trim(),
            Type = type,
            Balance = balance,
            InitialBalance = balance,
            Description = description,
            BankName = bankName
        };
    }

    public void Credit(Money amount)
    {
        if (!amount.IsPositive) throw new DomainException("Credit amount must be positive.");
        Balance = Balance.Add(amount);
        MarkUpdated();
    }

    public void Debit(Money amount)
    {
        if (!amount.IsPositive) throw new DomainException("Debit amount must be positive.");
        Balance = Balance.Subtract(amount);
        MarkUpdated();
    }

    public void UpdateDetails(string name, string? description, string? bankName, string color, string? iconName, bool includeInNetWorth)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Account name is required.");
        Name = name.Trim();
        Description = description;
        BankName = bankName;
        Color = color;
        IconName = iconName;
        IncludeInNetWorth = includeInNetWorth;
        MarkUpdated();
    }

    public void SetAsDefault() { IsDefault = true; MarkUpdated(); }
    public void ClearDefault() { IsDefault = false; MarkUpdated(); }
}
