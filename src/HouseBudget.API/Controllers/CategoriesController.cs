using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Enums;
using HouseBudget.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBudget.API.Controllers;

/// <summary>Manage transaction categories</summary>
[Authorize]
public sealed class CategoriesController : BaseApiController
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;

    public CategoriesController(ICategoryRepository categoryRepository, ICurrentUserService currentUser)
    {
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
    }

    /// <summary>Get all categories (system + user's custom)</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CategoryType? type = null, CancellationToken ct = default)
    {
        var categories = await _categoryRepository.GetAllForUserAsync(_currentUser.UserId, type, ct);
        return Success(categories.Select(c => new
        {
            c.Id, c.Name, c.Description, c.Type, TypeName = c.Type.ToString(), c.Color, c.IconName, c.IsSystem, c.ParentCategoryId
        }));
    }

    /// <summary>Create a custom category</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var category = Domain.Entities.Category.CreateForUser(_currentUser.UserId, request.Name, request.Type, request.Color, request.IconName, request.ParentCategoryId);
        await _categoryRepository.AddAsync(category, ct);
        return Created(new { category.Id, category.Name, category.Type, category.Color });
    }

    /// <summary>Update a custom category</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var cat = await _categoryRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Category", id);
        if (cat.UserId != _currentUser.UserId && !cat.IsSystem) throw new UnauthorizedDomainException();
        cat.Update(request.Name, request.Description, request.Color, request.IconName, request.Type);
        await _categoryRepository.UpdateAsync(cat, ct);
        return Success(new { cat.Id, cat.Name });
    }
}

public record CreateCategoryRequest(string Name, CategoryType Type, string Color = "#607D8B", string? IconName = null, Guid? ParentCategoryId = null);
public record UpdateCategoryRequest(string Name, string? Description, CategoryType Type, string Color, string? IconName);
