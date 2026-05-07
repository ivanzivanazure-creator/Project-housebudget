using HouseBudget.Application.DTOs.Auth;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Exceptions;
using MediatR;

namespace HouseBudget.Application.Commands.Auth;

public record RegisterCommand(string FirstName, string LastName, string Email, string Password, string Currency = "USD") : IRequest<AuthResponseDto>;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(IUserRepository userRepository, ITokenService tokenService, IEmailService emailService, IPasswordHasher hasher, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _emailService = emailService;
        _hasher = hasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
            throw new DomainException("An account with this email already exists.");

        var passwordHash = _hasher.Hash(request.Password);
        var user = User.Create(request.FirstName, request.LastName, request.Email, passwordHash, request.Currency);

        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        user.SetRefreshToken(refreshToken, refreshTokenExpiry);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _ = _emailService.SendWelcomeEmailAsync(user.Email.Value, user.FullName, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);

        return new AuthResponseDto(user.Id, user.Email.Value, user.FullName, accessToken, refreshToken, DateTime.UtcNow.AddHours(1), user.DefaultCurrency);
    }
}
