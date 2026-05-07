using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Events;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;

namespace HouseBudget.Domain.Entities;

public sealed class Budget : BaseEntity
{
    private readonly List<BudgetCategory> _budgetCategories = new();

    public Guid UserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public BudgetPeriodType PeriodType { get; private set; }
    public DateRange Period { get; private set; } = default!;
    public Money TotalAmount { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public bool RolloverUnspent { get; private set; }

    public IReadOnlyCollection<BudgetCategory> BudgetCategories => _budgetCategories.AsReadOnly();

    public Money TotalAllocated => Money.Of(
        _budgetCategories.Sum(bc => bc.AllocatedAmount.Amount),
        TotalAmount.Currency);

    public Money TotalSpent => Money.Of(
        _budgetCategories.Sum(bc => bc.SpentAmount.Amount),
        TotalAmount.Currency);

    public Money Remaining => TotalAmount.Subtract(TotalSpent);
    public decimal UsagePercentage => TotalAmount.Amount == 0 ? 0 : Math.Round(TotalSpent.Amount / TotalAmount.Amount * 100, 1);
    public bool IsOverBudget => TotalSpent > TotalAmount;

    private Budget() { }

    public static Budget Create(Guid userId, string name, BudgetPeriodType periodType, DateRange period, decimal totalAmount, string currency = "USD", string? description = null, bool rollover = false)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Budget name is required.");
        if (totalAmount <= 0) throw new DomainException("Budget amount must be positive.");

        var budget = new Budget
        {
            UserId = userId,
            Name = name.Trim(),
            Description = description,
            PeriodType = periodType,
            Period = period,
            TotalAmount = Money.Of(totalAmount, currency),
            RolloverUnspent = rollover
        };
        budget.AddDomainEvent(new BudgetCreatedEvent(budget.Id, userId, totalAmount));
        return budget;
    }

    public BudgetCategory AddCategory(Guid categoryId, decimal allocatedAmount)
    {
        if (_budgetCategories.Any(bc => bc.CategoryId == categoryId))
            throw new DomainException("Category already exists in this budget.");

        if (TotalAllocated.Amount + allocatedAmount > TotalAmount.Amount)
            throw new DomainException("Allocated amount exceeds budget total.");

        var bc = BudgetCategory.Create(Id, categoryId, allocatedAmount, TotalAmount.Currency);
        _budgetCategories.Add(bc);
        MarkUpdated();
        return bc;
    }

    public void RecordSpending(Guid categoryId, Money amount)
    {
        var bc = _budgetCategories.FirstOrDefault(x => x.CategoryId == categoryId)
            ?? throw new NotFoundException(nameof(BudgetCategory), categoryId);
        bc.RecordSpending(amount);

        if (IsOverBudget)
            AddDomainEvent(new BudgetExceededEvent(Id, UserId, TotalAmount.Amount, TotalSpent.Amount));
        MarkUpdated();
    }

    public void Update(string name, string? description, decimal totalAmount, bool rollover)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Budget name is required.");
        Name = name.Trim();
        Description = description;
        TotalAmount = Money.Of(totalAmount, TotalAmount.Currency);
        RolloverUnspent = rollover;
        MarkUpdated();
    }

    public void Deactivate() { IsActive = false; MarkUpdated(); }
}
