using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseBudget.Infrastructure.Repositories;

public sealed class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(u => u.Email.Value == email.ToLowerInvariant(), cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(u => u.Email.Value == email.ToLowerInvariant(), cancellationToken);
}
