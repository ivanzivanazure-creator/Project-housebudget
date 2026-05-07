using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Events;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;

namespace HouseBudget.Domain.Entities;

public sealed class Bill : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid AccountId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Money Amount { get; private set; } = default!;
    public RecurrenceType RecurrenceType { get; private set; }
    public DateOnly NextDueDate { get; private set; }
    public DateOnly? LastPaidDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool AutoPay { get; private set; }
    public int ReminderDaysBefore { get; private set; } = 3;
    public string? PayeeUrl { get; private set; }
    public string Color { get; private set; } = "#FF5722";

    public Account Account { get; private set; } = default!;
    public Category Category { get; private set; } = default!;

    public bool IsDueSoon => NextDueDate <= DateOnly.FromDateTime(DateTime.Today.AddDays(ReminderDaysBefore));
    public bool IsOverdue => NextDueDate < DateOnly.FromDateTime(DateTime.Today);

    private Bill() { }

    public static Bill Create(Guid userId, Guid accountId, Guid categoryId, string name, decimal amount, string currency, RecurrenceType recurrence, DateOnly nextDueDate, string? description = null, bool autoPay = false, int reminderDays = 3)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Bill name is required.");
        if (amount <= 0) throw new DomainException("Bill amount must be positive.");

        return new Bill
        {
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Name = name.Trim(),
            Description = description,
            Amount = Money.Of(amount, currency),
            RecurrenceType = recurrence,
            NextDueDate = nextDueDate,
            AutoPay = autoPay,
            ReminderDaysBefore = reminderDays
        };
    }

    public void MarkAsPaid(DateOnly paidDate)
    {
        LastPaidDate = paidDate;
        NextDueDate = CalculateNextDueDate(paidDate);
        AddDomainEvent(new BillPaidEvent(Id, UserId, Name, Amount.Amount));
        MarkUpdated();
    }

    public void Update(string name, string? description, decimal amount, RecurrenceType recurrence, DateOnly nextDueDate, bool autoPay, int reminderDays, string color)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Bill name is required.");
        Name = name.Trim();
        Description = description;
        Amount = Money.Of(amount, Amount.Currency);
        RecurrenceType = recurrence;
        NextDueDate = nextDueDate;
        AutoPay = autoPay;
        ReminderDaysBefore = reminderDays;
        Color = color;
        MarkUpdated();
    }

    public void Deactivate() { IsActive = false; MarkUpdated(); }

    private DateOnly CalculateNextDueDate(DateOnly fromDate) => RecurrenceType switch
    {
        RecurrenceType.Daily => fromDate.AddDays(1),
        RecurrenceType.Weekly => fromDate.AddDays(7),
        RecurrenceType.BiWeekly => fromDate.AddDays(14),
        RecurrenceType.Monthly => fromDate.AddMonths(1),
        RecurrenceType.Quarterly => fromDate.AddMonths(3),
        RecurrenceType.SemiAnnual => fromDate.AddMonths(6),
        RecurrenceType.Annual => fromDate.AddYears(1),
        _ => fromDate
    };
}
