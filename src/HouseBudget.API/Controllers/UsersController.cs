using HouseBudget.Application.Commands.Auth;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBudget.API.Controllers;

/// <summary>User profile and account management</summary>
[Authorize]
public sealed class UsersController : BaseApiController
{
    private readonly IUserRepository _userRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IUserRepository userRepo, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Get current user profile</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(_currentUser.UserId, ct)
            ?? throw new NotFoundException("User", _currentUser.UserId);

        return Success(new
        {
            user.Id,
            Email = user.Email.Value,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.PhoneNumber,
            user.DefaultCurrency,
            user.AvatarUrl,
            user.IsEmailVerified,
            user.LastLoginAt
        });
    }

    /// <summary>Update profile (name, phone, avatar, currency)</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(_currentUser.UserId, ct)
            ?? throw new NotFoundException("User", _currentUser.UserId);

        user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber, request.AvatarUrl);

        if (!string.IsNullOrEmpty(request.DefaultCurrency))
            user.SetDefaultCurrency(request.DefaultCurrency);

        await _unitOfWork.SaveChangesAsync(ct);
        return Success(new { user.FullName, user.DefaultCurrency }, "Profile updated.");
    }

    /// <summary>Change password</summary>
    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await Mediator.Send(
            new ChangePasswordCommand(request.CurrentPassword, request.NewPassword, request.ConfirmPassword), ct);
        return Success<object?>(null, "Password changed successfully.");
    }

    /// <summary>Deactivate (soft-delete) account</summary>
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount(CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(_currentUser.UserId, ct)
            ?? throw new NotFoundException("User", _currentUser.UserId);

        user.Deactivate();
        await _unitOfWork.SaveChangesAsync(ct);
        return Success<object?>(null, "Account deactivated.");
    }
}

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    string? AvatarUrl = null,
    string? DefaultCurrency = null);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);
