using FluentAssertions;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Events;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;
using Xunit;

namespace HouseBudget.Domain.Tests;

public sealed class BudgetTests
{
    private static Budget CreateTestBudget(decimal total = 2000)
        => Budget.Create(Guid.NewGuid(), "Monthly Budget", BudgetPeriodType.Monthly, DateRange.CurrentMonth(), total, "USD");

    [Fact]
    public void Create_ValidData_CreatesBudgetCorrectly()
    {
        var budget = CreateTestBudget(2000);
        budget.TotalAmount.Amount.Should().Be(2000);
        budget.IsActive.Should().BeTrue();
        budget.IsOverBudget.Should().BeFalse();
        budget.DomainEvents.Should().ContainSingle(e => e is BudgetCreatedEvent);
    }

    [Fact]
    public void AddCategory_ValidCategory_AddsToBudget()
    {
        var budget = CreateTestBudget(1000);
        var categoryId = Guid.NewGuid();
        var bc = budget.AddCategory(categoryId, 300);
        bc.AllocatedAmount.Amount.Should().Be(300);
        budget.BudgetCategories.Should().HaveCount(1);
        budget.TotalAllocated.Amount.Should().Be(300);
    }

    [Fact]
    public void AddCategory_ExceedsTotalBudget_ThrowsDomainException()
    {
        var budget = CreateTestBudget(500);
        var act = () => budget.AddCategory(Guid.NewGuid(), 600);
        act.Should().Throw<DomainException>().WithMessage("*exceeds*");
    }

    [Fact]
    public void AddCategory_DuplicateCategory_ThrowsDomainException()
    {
        var budget = CreateTestBudget(1000);
        var categoryId = Guid.NewGuid();
        budget.AddCategory(categoryId, 200);
        var act = () => budget.AddCategory(categoryId, 100);
        act.Should().Throw<DomainException>().WithMessage("*already exists*");
    }

    [Fact]
    public void RecordSpending_ExceedsBudget_RaisesBudgetExceededEvent()
    {
        var budget = CreateTestBudget(500);
        var categoryId = Guid.NewGuid();
        budget.AddCategory(categoryId, 300);
        budget.ClearDomainEvents();

        budget.RecordSpending(categoryId, Money.Of(600, "USD"));

        budget.IsOverBudget.Should().BeTrue();
        budget.DomainEvents.Should().ContainSingle(e => e is BudgetExceededEvent);
    }
}
