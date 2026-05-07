using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Interfaces;

namespace HouseBudget.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetAllForUserAsync(Guid userId, CategoryType? type = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetSystemCategoriesAsync(CategoryType? type = null, CancellationToken cancellationToken = default);
}
