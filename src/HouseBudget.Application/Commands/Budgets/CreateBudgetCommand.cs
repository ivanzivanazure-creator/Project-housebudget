using HouseBudget.Application.DTOs.Budgets;
using HouseBudget.Application.Interfaces;
using HouseBudget.Domain.Entities;
using HouseBudget.Domain.Enums;
using HouseBudget.Domain.ValueObjects;
using MediatR;

namespace HouseBudget.Application.Commands.Budgets;

public record BudgetCategoryInput(Guid CategoryId, decimal AllocatedAmount);

public record CreateBudgetCommand(
    string Name,
    BudgetPeriodType PeriodType,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal TotalAmount,
    string Currency,
    string? Description = null,
    bool RolloverUnspent = false,
    List<BudgetCategoryInput>? Categories = null
) : IRequest<BudgetDto>;

public sealed class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, BudgetDto>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBudgetCommandHandler(IBudgetRepository budgetRepository, ICategoryRepository categoryRepository, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<BudgetDto> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var period = DateRange.Of(request.PeriodStart, request.PeriodEnd);
        var budget = Budget.Create(_currentUser.UserId, request.Name, request.PeriodType, period, request.TotalAmount, request.Currency, request.Description, request.RolloverUnspent);

        var categoryDtos = new List<BudgetCategoryDto>();

        if (request.Categories?.Any() == true)
        {
            foreach (var cat in request.Categories)
            {
                var category = await _categoryRepository.GetByIdAsync(cat.CategoryId, cancellationToken);
                if (category is null) continue;
                var bc = budget.AddCategory(cat.CategoryId, cat.AllocatedAmount);
                categoryDtos.Add(new BudgetCategoryDto(bc.Id, cat.CategoryId, category.Name, category.Color, cat.AllocatedAmount, 0, cat.AllocatedAmount, 0, false));
            }
        }

        await _budgetRepository.AddAsync(budget, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BudgetDto(budget.Id, budget.Name, budget.Description, budget.PeriodType, budget.Period.Start, budget.Period.End,
            budget.TotalAmount.Amount, budget.TotalAllocated.Amount, budget.TotalSpent.Amount, budget.Remaining.Amount,
            budget.UsagePercentage, budget.IsOverBudget, budget.TotalAmount.Currency, budget.IsActive, categoryDtos, budget.CreatedAt);
    }
}
