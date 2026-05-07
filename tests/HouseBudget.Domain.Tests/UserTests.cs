using FluentAssertions;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Events;
using HouseBudget.Domain.Exceptions;
using Xunit;

namespace HouseBudget.Domain.Tests;

public sealed class UserTests
{
    [Fact]
    public void Create_ValidData_CreatesUserWithCorrectProperties()
    {
        var user = User.Create("John", "Doe", "john.doe@example.com", "hashedpw123", "USD");

        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Email.Value.Should().Be("john.doe@example.com");
        user.FullName.Should().Be("John Doe");
        user.DefaultCurrency.Should().Be("USD");
        user.IsActive.Should().BeTrue();
        user.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public void Create_RaisesUserRegisteredDomainEvent()
    {
        var user = User.Create("Jane", "Smith", "jane@example.com", "hash", "EUR");
        user.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<UserRegisteredEvent>();
    }

    [Fact]
    public void Create_EmptyFirstName_ThrowsDomainException()
    {
        var act = () => User.Create("", "Doe", "test@test.com", "hash");
        act.Should().Throw<DomainException>().WithMessage("*First name*");
    }

    [Fact]
    public void Create_InvalidEmail_ThrowsDomainException()
    {
        var act = () => User.Create("John", "Doe", "notanemail", "hash");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void VerifyEmail_SetsEmailVerified()
    {
        var user = User.Create("John", "Doe", "john@test.com", "hash");
        user.VerifyEmail();
        user.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public void SetRefreshToken_StoresToken()
    {
        var user = User.Create("John", "Doe", "john@test.com", "hash");
        var expiry = DateTime.UtcNow.AddDays(30);
        user.SetRefreshToken("mytoken", expiry);
        user.RefreshToken.Should().Be("mytoken");
        user.RefreshTokenExpiry.Should().Be(expiry);
    }
}
