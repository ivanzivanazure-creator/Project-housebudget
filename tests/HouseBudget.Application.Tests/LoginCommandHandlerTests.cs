using FluentAssertions;
using HouseBudget.Application.Commands.Auth;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.Interfaces;
using Moq;
using Xunit;

namespace HouseBudget.Application.Tests;

public sealed class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private LoginCommandHandler CreateHandler() =>
        new(_userRepoMock.Object, _tokenServiceMock.Object, _hasherMock.Object, _unitOfWorkMock.Object);

    private static User MakeUser()
    {
        var u = User.Create("Ivan", "Test", "ivan@example.com", "hashed", "USD");
        return u;
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokens()
    {
        var user = MakeUser();
        _userRepoMock.Setup(r => r.GetByEmailAsync("ivan@example.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("Password1!", "hashed")).Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns("at");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("rt");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await CreateHandler().Handle(new LoginCommand("ivan@example.com", "Password1!"), default);

        result.AccessToken.Should().Be("at");
        result.RefreshToken.Should().Be("rt");
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsAndRecordsFailedAttempt()
    {
        var user = MakeUser();
        _userRepoMock.Setup(r => r.GetByEmailAsync("ivan@example.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var act = () => CreateHandler().Handle(new LoginCommand("ivan@example.com", "wrong"), default);

        await act.Should().ThrowAsync<UnauthorizedDomainException>().WithMessage("*Invalid*");
        user.FailedLoginAttempts.Should().Be(1);
    }

    [Fact]
    public async Task Handle_LockedAccount_ThrowsBeforeCheckingPassword()
    {
        var user = MakeUser();
        // Force lockout by exceeding max attempts
        for (var i = 0; i < 5; i++) user.RecordFailedLogin();

        _userRepoMock.Setup(r => r.GetByEmailAsync("ivan@example.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var act = () => CreateHandler().Handle(new LoginCommand("ivan@example.com", "Password1!"), default);

        await act.Should().ThrowAsync<UnauthorizedDomainException>().WithMessage("*locked*");
        _hasherMock.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnknownEmail_ThrowsUnauthorized()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);

        var act = () => CreateHandler().Handle(new LoginCommand("no@example.com", "pass"), default);

        await act.Should().ThrowAsync<UnauthorizedDomainException>();
    }
}
