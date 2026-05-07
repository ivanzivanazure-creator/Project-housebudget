using HouseBudget.Application.Queries.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBudget.API.Controllers;

/// <summary>Financial reports and analytics</summary>
[Authorize]
public sealed class ReportsController : BaseApiController
{
    /// <summary>Get monthly income/expense report</summary>
    [HttpGet("monthly/{year:int}/{month:int}")]
    public async Task<IActionResult> GetMonthly(int year, int month, CancellationToken ct)
        => Success(await Mediator.Send(new GetMonthlyReportQuery(year, month), ct));

    /// <summary>Get annual report</summary>
    [HttpGet("annual/{year:int}")]
    public async Task<IActionResult> GetAnnual(int year, CancellationToken ct)
        => Success(await Mediator.Send(new GetAnnualReportQuery(year), ct));
}
