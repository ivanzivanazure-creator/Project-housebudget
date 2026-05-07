using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseBudget.Infrastructure.Repositories;

public sealed class AccountRepository : BaseRepository<Account>, IAccountRepository
{
    public AccountRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Account>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await DbSet.Where(a => a.UserId == userId).OrderBy(a => a.Name).ToListAsync(cancellationToken);

    public async Task<Account?> GetDefaultAccountAsync(Guid userId, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault, cancellationToken);
}
