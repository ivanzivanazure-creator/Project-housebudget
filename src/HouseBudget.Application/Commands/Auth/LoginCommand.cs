using HouseBudget.Application.DTOs.Auth;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Exceptions;
using MediatR;

namespace HouseBudget.Application.Commands.Auth;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(IUserRepository userRepository, ITokenService tokenService, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedDomainException("Invalid email or password.");

        if (!user.IsActive) throw new UnauthorizedDomainException("Account is deactivated.");
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedDomainException("Invalid email or password.");

        var refreshToken = _tokenService.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(30));
        user.RecordLogin();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);
        return new AuthResponseDto(user.Id, user.Email.Value, user.FullName, accessToken, refreshToken, DateTime.UtcNow.AddHours(1), user.DefaultCurrency);
    }
}
