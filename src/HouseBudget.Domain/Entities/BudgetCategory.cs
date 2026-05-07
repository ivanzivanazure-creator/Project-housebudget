using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;

namespace HouseBudget.Domain.Entities;

public sealed class BudgetCategory : BaseEntity
{
    public Guid BudgetId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Money AllocatedAmount { get; private set; } = default!;
    public Money SpentAmount { get; private set; } = default!;

    public Budget Budget { get; private set; } = default!;
    public Category Category { get; private set; } = default!;

    public Money Remaining => AllocatedAmount.Subtract(SpentAmount);
    public decimal UsagePercentage => AllocatedAmount.Amount == 0 ? 0 : Math.Round(SpentAmount.Amount / AllocatedAmount.Amount * 100, 1);
    public bool IsOverBudget => SpentAmount > AllocatedAmount;

    private BudgetCategory() { }

    public static BudgetCategory Create(Guid budgetId, Guid categoryId, decimal allocatedAmount, string currency)
    {
        if (allocatedAmount < 0) throw new DomainException("Allocated amount cannot be negative.");
        return new BudgetCategory
        {
            BudgetId = budgetId,
            CategoryId = categoryId,
            AllocatedAmount = Money.Of(allocatedAmount, currency),
            SpentAmount = Money.Zero(currency)
        };
    }

    public void RecordSpending(Money amount)
    {
        SpentAmount = SpentAmount.Add(amount);
        MarkUpdated();
    }

    public void UpdateAllocation(decimal amount, string currency)
    {
        AllocatedAmount = Money.Of(amount, currency);
        MarkUpdated();
    }
}
