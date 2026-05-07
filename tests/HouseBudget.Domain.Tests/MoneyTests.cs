using FluentAssertions;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.ValueObjects;
using Xunit;

namespace HouseBudget.Domain.Tests;

public sealed class MoneyTests
{
    [Fact]
    public void Of_ValidAmount_CreatesMoneyCorrectly()
    {
        var money = Money.Of(100.50m, "USD");
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Of_InvalidCurrency_ThrowsDomainException()
    {
        var act = () => Money.Of(100, "US");
        act.Should().Throw<DomainException>().WithMessage("*ISO*");
    }

    [Fact]
    public void Add_SameCurrency_ReturnsCorrectSum()
    {
        var a = Money.Of(100, "USD");
        var b = Money.Of(50, "USD");
        var result = a.Add(b);
        result.Amount.Should().Be(150);
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsDomainException()
    {
        var a = Money.Of(100, "USD");
        var b = Money.Of(50, "EUR");
        var act = () => a.Add(b);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsCorrectDifference()
    {
        var a = Money.Of(100, "USD");
        var b = Money.Of(30, "USD");
        a.Subtract(b).Amount.Should().Be(70);
    }

    [Fact]
    public void Equals_SameAmountAndCurrency_ReturnsTrue()
    {
        var a = Money.Of(100, "USD");
        var b = Money.Of(100, "USD");
        a.Should().Be(b);
    }

    [Fact]
    public void IsPositive_PositiveAmount_ReturnsTrue()
    {
        Money.Of(1, "USD").IsPositive.Should().BeTrue();
        Money.Of(-1, "USD").IsPositive.Should().BeFalse();
    }

    [Fact]
    public void Zero_ReturnsZeroAmount()
    {
        var zero = Money.Zero("USD");
        zero.IsZero.Should().BeTrue();
        zero.Amount.Should().Be(0);
    }
}
