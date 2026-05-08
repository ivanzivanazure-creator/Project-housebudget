using FluentAssertions;
using HouseBudget.Domain.Entities;
using Xunit;

namespace HouseBudget.Domain.Tests;

public sealed class UserLockoutTests
{
    private static User MakeUser() =>
        User.Create("Ivan", "Test", "ivan@example.com", "hash", "USD");

    [Fact]
    public void RecordFailedLogin_BelowMax_DoesNotLock()
    {
        var user = MakeUser();

        for (var i = 0; i < 4; i++)
            user.RecordFailedLogin();

        user.IsLockedOut.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(4);
    }

    [Fact]
    public void RecordFailedLogin_AtMax_LocksAccount()
    {
        var user = MakeUser();

        for (var i = 0; i < 5; i++)
            user.RecordFailedLogin();

        user.IsLockedOut.Should().BeTrue();
        user.LockoutEnd.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void RecordLogin_AfterFailures_ResetsCounter()
    {
        var user = MakeUser();

        for (var i = 0; i < 3; i++)
            user.RecordFailedLogin();

        user.RecordLogin();

        user.FailedLoginAttempts.Should().Be(0);
        user.IsLockedOut.Should().BeFalse();
    }

    [Fact]
    public void IsLockedOut_AfterLockoutExpires_ReturnsFalse()
    {
        var user = MakeUser();
        // Lock with 0-minute lockout (already in the past)
        for (var i = 0; i < 5; i++)
            user.RecordFailedLogin(maxAttempts: 5, lockoutMinutes: 0);

        // LockoutEnd = UtcNow + 0 min = essentially now, so should not be locked
        user.IsLockedOut.Should().BeFalse();
    }
}
