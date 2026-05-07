using HouseBudget.Application.Commands.Budgets;
using HouseBudget.Application.DTOs.Budgets;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBudget.API.Controllers;

/// <summary>Manage budgets and budget categories</summary>
[Authorize]
public sealed class BudgetsController : BaseApiController
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;

    public BudgetsController(IBudgetRepository budgetRepository, ICategoryRepository categoryRepository, ICurrentUserService currentUser)
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
    }

    /// <summary>Get all budgets for current user</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false, CancellationToken ct = default)
    {
        var budgets = await _budgetRepository.GetByUserIdAsync(_currentUser.UserId, activeOnly, ct);
        var categories = await _categoryRepository.GetAllForUserAsync(_currentUser.UserId, null, ct);

        var dtos = budgets.Select(b => new BudgetDto(
            b.Id, b.Name, b.Description, b.PeriodType, b.Period.Start, b.Period.End,
            b.TotalAmount.Amount, b.TotalAllocated.Amount, b.TotalSpent.Amount, b.Remaining.Amount,
            b.UsagePercentage, b.IsOverBudget, b.TotalAmount.Currency, b.IsActive,
            b.BudgetCategories.Select(bc =>
            {
                var cat = categories.FirstOrDefault(c => c.Id == bc.CategoryId);
                return new BudgetCategoryDto(bc.Id, bc.CategoryId, cat?.Name ?? "Unknown", cat?.Color ?? "#607D8B",
                    bc.AllocatedAmount.Amount, bc.SpentAmount.Amount, bc.Remaining.Amount, bc.UsagePercentage, bc.IsOverBudget);
            }).ToList(),
            b.CreatedAt));

        return Success(dtos);
    }

    /// <summary>Get a budget by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var b = await _budgetRepository.GetWithCategoriesAsync(id, ct) ?? throw new NotFoundException("Budget", id);
        if (b.UserId != _currentUser.UserId) throw new UnauthorizedDomainException();
        var categories = await _categoryRepository.GetAllForUserAsync(_currentUser.UserId, null, ct);

        var dto = new BudgetDto(b.Id, b.Name, b.Description, b.PeriodType, b.Period.Start, b.Period.End,
            b.TotalAmount.Amount, b.TotalAllocated.Amount, b.TotalSpent.Amount, b.Remaining.Amount,
            b.UsagePercentage, b.IsOverBudget, b.TotalAmount.Currency, b.IsActive,
            b.BudgetCategories.Select(bc => { var cat = categories.FirstOrDefault(c => c.Id == bc.CategoryId); return new BudgetCategoryDto(bc.Id, bc.CategoryId, cat?.Name ?? "Unknown", cat?.Color ?? "#607D8B", bc.AllocatedAmount.Amount, bc.SpentAmount.Amount, bc.Remaining.Amount, bc.UsagePercentage, bc.IsOverBudget); }).ToList(),
            b.CreatedAt);

        return Success(dto);
    }

    /// <summary>Create a new budget</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBudgetCommand command, CancellationToken ct)
        => Created(await Mediator.Send(command, ct));
}
