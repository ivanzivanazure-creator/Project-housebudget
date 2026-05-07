using HouseBudget.Application.Commands.Subscriptions;
using HouseBudget.Application.Queries.Subscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBudget.API.Controllers;

/// <summary>Manage user subscriptions and plan selection</summary>
[Authorize]
public sealed class SubscriptionsController : BaseApiController
{
    /// <summary>Get all available subscription plans</summary>
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans(CancellationToken ct)
        => Success(await Mediator.Send(new GetSubscriptionPlansQuery(), ct));

    /// <summary>Get current user's active subscription</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
        => Success(await Mediator.Send(new GetMySubscriptionQuery(), ct));

    /// <summary>Get full subscription history</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
        => Success(await Mediator.Send(new GetSubscriptionHistoryQuery(), ct));

    /// <summary>Subscribe to a plan (creates trial or active subscription)</summary>
    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] CreateSubscriptionCommand command, CancellationToken ct)
        => Created(await Mediator.Send(command, ct));

    /// <summary>Upgrade or downgrade to a different plan</summary>
    [HttpPut("upgrade")]
    public async Task<IActionResult> Upgrade([FromBody] UpgradeSubscriptionCommand command, CancellationToken ct)
        => Success(await Mediator.Send(command, ct));

    /// <summary>Cancel current subscription</summary>
    [HttpDelete]
    public async Task<IActionResult> Cancel([FromBody] CancelSubscriptionRequest request, CancellationToken ct)
        => Success(await Mediator.Send(new CancelSubscriptionCommand(request.Reason, request.AtPeriodEnd), ct));
}

public record CancelSubscriptionRequest(string? Reason = null, bool AtPeriodEnd = true);
