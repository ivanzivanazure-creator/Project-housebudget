using HouseBudget.Application.Commands.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBudget.API.Controllers;

/// <summary>Authentication endpoints</summary>
public sealed class AuthController : BaseApiController
{
    /// <summary>Register a new user account</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
        => Created(await Mediator.Send(command, ct));

    /// <summary>Login with email and password</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
        => Success(await Mediator.Send(command, ct));

    /// <summary>Refresh access token</summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken ct)
        => Success(await Mediator.Send(command, ct));
}
