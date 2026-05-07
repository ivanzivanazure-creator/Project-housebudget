using FluentAssertions;
using HouseBudget.Application.Commands.Auth;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Exceptions;
using HouseBudget.Domain.Interfaces;
using Moq;
using Xunit;

namespace HouseBudget.Application.Tests;

public sealed class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private RegisterCommandHandler CreateHandler() =>
        new(_userRepoMock.Object, _tokenServiceMock.Object, _emailServiceMock.Object, _unitOfWorkMock.Object);

    [Fact]
    public async Task Handle_NewUser_ReturnsAuthResponse()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>(), default)).ReturnsAsync((User u, CancellationToken _) => u);
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access_token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh_token");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new RegisterCommand("John", "Doe", "john@example.com", "Password1!", "USD");

        var result = await handler.Handle(command, default);

        result.Should().NotBeNull();
        result.Email.Should().Be("john@example.com");
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task Handle_ExistingEmail_ThrowsDomainException()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync("existing@example.com", default)).ReturnsAsync(true);

        var handler = CreateHandler();
        var command = new RegisterCommand("Jane", "Doe", "existing@example.com", "Password1!");

        var act = () => handler.Handle(command, default);
        await act.Should().ThrowAsync<DomainException>().WithMessage("*already exists*");
    }
}
