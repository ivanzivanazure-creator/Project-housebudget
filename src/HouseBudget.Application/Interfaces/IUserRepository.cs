using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
