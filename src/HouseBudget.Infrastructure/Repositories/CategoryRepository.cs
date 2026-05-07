using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseBudget.Infrastructure.Repositories;

public sealed class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Category>> GetAllForUserAsync(Guid userId, CategoryType? type = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(c => c.UserId == userId || c.IsSystem);
        if (type.HasValue) query = query.Where(c => c.Type == type.Value || c.Type == CategoryType.Both);
        return await query.OrderBy(c => c.IsSystem).ThenBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetSystemCategoriesAsync(CategoryType? type = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(c => c.IsSystem);
        if (type.HasValue) query = query.Where(c => c.Type == type.Value || c.Type == CategoryType.Both);
        return await query.OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }
}
