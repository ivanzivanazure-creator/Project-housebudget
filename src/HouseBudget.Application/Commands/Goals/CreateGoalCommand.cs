using HouseBudget.Application.DTOs.Goals;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using MediatR;

namespace HouseBudget.Application.Commands.Goals;

public record CreateGoalCommand(string Name, decimal TargetAmount, string Currency, DateOnly? TargetDate = null, string? Description = null, string Color = "#4CAF50", string? IconName = null) : IRequest<GoalDto>;

public sealed class CreateGoalCommandHandler : IRequestHandler<CreateGoalCommand, GoalDto>
{
    private readonly IGoalRepository _goalRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGoalCommandHandler(IGoalRepository goalRepository, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<GoalDto> Handle(CreateGoalCommand request, CancellationToken cancellationToken)
    {
        var goal = Goal.Create(_currentUser.UserId, request.Name, request.TargetAmount, request.Currency, request.TargetDate, request.Description);
        goal.Update(request.Name, request.Description, request.TargetAmount, request.TargetDate, request.Color, request.IconName);

        await _goalRepository.AddAsync(goal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(goal);
    }

    public static GoalDto MapToDto(Goal g) => new(
        g.Id, g.Name, g.Description, g.TargetAmount.Amount, g.CurrentAmount.Amount,
        g.Remaining.Amount, g.ProgressPercentage, g.TargetAmount.Currency, g.TargetDate,
        g.Status, g.Status.ToString(), g.Color, g.IconName,
        g.MonthlyRequired?.Amount, g.CreatedAt);
}
