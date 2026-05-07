using HouseBudget.Application.Commands.Goals;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBudget.API.Controllers;

/// <summary>Manage savings goals</summary>
[Authorize]
public sealed class GoalsController : BaseApiController
{
    private readonly IGoalRepository _goalRepository;
    private readonly ICurrentUserService _currentUser;

    public GoalsController(IGoalRepository goalRepository, ICurrentUserService currentUser)
    {
        _goalRepository = goalRepository;
        _currentUser = currentUser;
    }

    /// <summary>Get all goals for current user</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GoalStatus? status = null, CancellationToken ct = default)
    {
        var goals = await _goalRepository.GetByUserIdAsync(_currentUser.UserId, status, ct);
        return Success(goals.Select(CreateGoalCommandHandler.MapToDto));
    }

    /// <summary>Get a goal by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var goal = await _goalRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Goal", id);
        if (goal.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();
        return Success(CreateGoalCommandHandler.MapToDto(goal));
    }

    /// <summary>Create a savings goal</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoalCommand command, CancellationToken ct)
        => Created(await Mediator.Send(command, ct));

    /// <summary>Contribute to a goal</summary>
    [HttpPost("{id:guid}/contribute")]
    public async Task<IActionResult> Contribute(Guid id, [FromBody] ContributeRequest request, CancellationToken ct)
        => Success(await Mediator.Send(new ContributeToGoalCommand(id, request.Amount, request.Note), ct));

    /// <summary>Pause a goal</summary>
    [HttpPatch("{id:guid}/pause")]
    public async Task<IActionResult> Pause(Guid id, CancellationToken ct)
    {
        var goal = await _goalRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Goal", id);
        if (goal.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();
        goal.Pause();
        await _goalRepository.UpdateAsync(goal, ct);
        return Success(CreateGoalCommandHandler.MapToDto(goal));
    }
}

public record ContributeRequest(decimal Amount, string? Note = null);
