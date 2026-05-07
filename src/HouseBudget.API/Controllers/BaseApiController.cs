using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HouseBudget.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult Success<T>(T data, string? message = null) =>
        Ok(new { success = true, message, data });

    protected IActionResult Created<T>(T data, string? location = null) =>
        StatusCode(201, new { success = true, data });
}
