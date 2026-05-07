using FluentAssertions;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Events;
using HouseBudget.Domain.Exceptions;
using Xunit;

namespace HouseBudget.Domain.Tests;

public sealed class GoalTests
{
    private static Goal CreateTestGoal(decimal target = 1000m)
        => Goal.Create(Guid.NewGuid(), "Emergency Fund", target, "USD");

    [Fact]
    public void Create_ValidData_CreatesGoalWithZeroProgress()
    {
        var goal = CreateTestGoal(1000);
        goal.TargetAmount.Amount.Should().Be(1000);
        goal.CurrentAmount.Amount.Should().Be(0);
        goal.ProgressPercentage.Should().Be(0);
        goal.Status.Should().Be(GoalStatus.Active);
    }

    [Fact]
    public void Contribute_ValidAmount_IncreasesCurrentAmount()
    {
        var goal = CreateTestGoal(1000);
        goal.Contribute(500);
        goal.CurrentAmount.Amount.Should().Be(500);
        goal.ProgressPercentage.Should().Be(50);
    }

    [Fact]
    public void Contribute_FullAmount_CompletesGoalAndRaisesDomainEvent()
    {
        var goal = CreateTestGoal(500);
        goal.Contribute(500);
        goal.Status.Should().Be(GoalStatus.Completed);
        goal.DomainEvents.Should().ContainSingle(e => e is GoalAchievedEvent);
    }

    [Fact]
    public void Contribute_NegativeAmount_ThrowsDomainException()
    {
        var goal = CreateTestGoal();
        var act = () => goal.Contribute(-100);
        act.Should().Throw<DomainException>().WithMessage("*positive*");
    }

    [Fact]
    public void Pause_ActiveGoal_ChangesStatusToPaused()
    {
        var goal = CreateTestGoal();
        goal.Pause();
        goal.Status.Should().Be(GoalStatus.Paused);
    }

    [Fact]
    public void Contribute_PausedGoal_ThrowsDomainException()
    {
        var goal = CreateTestGoal();
        goal.Pause();
        var act = () => goal.Contribute(100);
        act.Should().Throw<DomainException>().WithMessage("*non-active*");
    }

    [Fact]
    public void Remaining_PartialContribution_ShowsCorrectRemainder()
    {
        var goal = CreateTestGoal(1000);
        goal.Contribute(300);
        goal.Remaining.Amount.Should().Be(700);
    }
}
