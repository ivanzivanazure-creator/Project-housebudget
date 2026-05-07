using HouseBudget.Domain.ValueObjects;

namespace HouseBudget.Domain.Entities;

public sealed class GoalContribution : BaseEntity
{
    public Guid GoalId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public string? Note { get; private set; }
    public DateOnly ContributionDate { get; private set; }

    public Goal Goal { get; private set; } = default!;

    private GoalContribution() { }

    public static GoalContribution Create(Guid goalId, decimal amount, string currency, string? note)
    {
        return new GoalContribution
        {
            GoalId = goalId,
            Amount = Money.Of(amount, currency),
            Note = note,
            ContributionDate = DateOnly.FromDateTime(DateTime.Today)
        };
    }
}
