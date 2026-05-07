using HouseBudget.Application.DTOs.Auth;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Exceptions;
using MediatR;

namespace HouseBudget.Application.Commands.Auth;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponseDto>;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(IUserRepository userRepository, ITokenService tokenService, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = _tokenService.GetUserIdFromToken(request.AccessToken)
            ?? throw new UnauthorizedDomainException("Invalid access token.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedDomainException("User not found.");

        if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedDomainException("Invalid or expired refresh token.");

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(30));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);
        return new AuthResponseDto(user.Id, user.Email.Value, user.FullName, accessToken, newRefreshToken, DateTime.UtcNow.AddHours(1), user.DefaultCurrency);
    }
}
