namespace HouseBudget.Application.DTOs.Auth;

public record AuthResponseDto(
    Guid UserId,
    string Email,
    string FullName,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string DefaultCurrency
);
