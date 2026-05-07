using HouseBudget.Application.DTOs.Goals;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Exceptions;
using MediatR;

namespace HouseBudget.Application.Commands.Goals;

public record ContributeToGoalCommand(Guid GoalId, decimal Amount, string? Note = null) : IRequest<GoalDto>;

public sealed class ContributeToGoalCommandHandler : IRequestHandler<ContributeToGoalCommand, GoalDto>
{
    private readonly IGoalRepository _goalRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ContributeToGoalCommandHandler(IGoalRepository goalRepository, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<GoalDto> Handle(ContributeToGoalCommand request, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(request.GoalId, cancellationToken)
            ?? throw new NotFoundException(nameof(Goal), request.GoalId);

        if (goal.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();

        goal.Contribute(request.Amount, request.Note);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreateGoalCommandHandler.MapToDto(goal);
    }
}
