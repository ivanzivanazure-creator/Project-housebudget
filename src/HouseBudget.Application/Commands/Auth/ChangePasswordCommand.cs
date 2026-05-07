using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Exceptions;
using MediatR;

namespace HouseBudget.Application.Commands.Auth;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmPassword) : IRequest;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IUserRepository _userRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(IUserRepository userRepo, ICurrentUserService currentUser, IPasswordHasher hasher, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _currentUser = currentUser;
        _hasher = hasher;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.ConfirmPassword)
            throw new DomainException("New passwords do not match.");

        var user = await _userRepo.GetByIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("User", _currentUser.UserId);

        if (!_hasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedDomainException("Current password is incorrect.");

        user.ChangePassword(_hasher.Hash(request.NewPassword));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
