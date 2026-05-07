using HouseBudget.Application.Queries.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBudget.API.Controllers;

/// <summary>Financial dashboard overview</summary>
[Authorize]
public sealed class DashboardController : BaseApiController
{
    /// <summary>Get complete dashboard data</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Success(await Mediator.Send(new GetDashboardQuery(), ct));
}
