using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Events;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;

namespace HouseBudget.Domain.Entities;

public sealed class Goal : BaseEntity
{
    private readonly List<GoalContribution> _contributions = new();

    public Guid UserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Money TargetAmount { get; private set; } = default!;
    public Money CurrentAmount { get; private set; } = default!;
    public DateOnly? TargetDate { get; private set; }
    public GoalStatus Status { get; private set; } = GoalStatus.Active;
    public string Color { get; private set; } = "#4CAF50";
    public string? IconName { get; private set; }
    public Guid? LinkedAccountId { get; private set; }

    public IReadOnlyCollection<GoalContribution> Contributions => _contributions.AsReadOnly();

    public decimal ProgressPercentage => TargetAmount.Amount == 0 ? 0 : Math.Min(100, Math.Round(CurrentAmount.Amount / TargetAmount.Amount * 100, 1));
    public bool IsCompleted => Status == GoalStatus.Completed;
    public Money Remaining => TargetAmount.Subtract(CurrentAmount).Amount < 0 ? Money.Zero(TargetAmount.Currency) : TargetAmount.Subtract(CurrentAmount);
    public Money? MonthlyRequired => TargetDate.HasValue && TargetDate.Value > DateOnly.FromDateTime(DateTime.Today)
        ? Money.Of(Math.Max(0, Remaining.Amount / Math.Max(1, MonthsRemaining())), TargetAmount.Currency)
        : null;

    private int MonthsRemaining()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return (TargetDate!.Value.Year - today.Year) * 12 + TargetDate.Value.Month - today.Month;
    }

    private Goal() { }

    public static Goal Create(Guid userId, string name, decimal targetAmount, string currency, DateOnly? targetDate = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Goal name is required.");
        if (targetAmount <= 0) throw new DomainException("Target amount must be positive.");

        return new Goal
        {
            UserId = userId,
            Name = name.Trim(),
            Description = description,
            TargetAmount = Money.Of(targetAmount, currency),
            CurrentAmount = Money.Zero(currency),
            TargetDate = targetDate
        };
    }

    public GoalContribution Contribute(decimal amount, string? note = null)
    {
        if (Status != GoalStatus.Active) throw new DomainException("Cannot contribute to a non-active goal.");
        if (amount <= 0) throw new DomainException("Contribution amount must be positive.");

        var money = Money.Of(amount, TargetAmount.Currency);
        CurrentAmount = CurrentAmount.Add(money);

        var contribution = GoalContribution.Create(Id, amount, TargetAmount.Currency, note);
        _contributions.Add(contribution);

        if (CurrentAmount >= TargetAmount)
        {
            Status = GoalStatus.Completed;
            AddDomainEvent(new GoalAchievedEvent(Id, UserId, Name, TargetAmount.Amount));
        }

        MarkUpdated();
        return contribution;
    }

    public void Update(string name, string? description, decimal targetAmount, DateOnly? targetDate, string color, string? iconName)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Goal name is required.");
        Name = name.Trim();
        Description = description;
        TargetAmount = Money.Of(targetAmount, TargetAmount.Currency);
        TargetDate = targetDate;
        Color = color;
        IconName = iconName;
        MarkUpdated();
    }

    public void Pause() { Status = GoalStatus.Paused; MarkUpdated(); }
    public void Resume() { Status = GoalStatus.Active; MarkUpdated(); }
    public void Cancel() { Status = GoalStatus.Cancelled; MarkUpdated(); }
}
